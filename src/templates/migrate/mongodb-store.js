'use strict';
const MongoClient = require('mongodb').MongoClient;
const database = require('./db').database;
const connectionString = process.env.CONNECTION_STRING;

class MongoDbStore {
  async load(fn) {
    let client = null;
    let data = null;
    try {
      client = await MongoClient.connect(connectionString);
      const db = client.db(database);
      data = await db.collection('migrations').find().toArray();
      if (data.length !== 1) {
        console.log('Cannot read migrations from database. If this is the first time you run migrations, then this is normal.');
        return fn(null, {});
      }
    } catch (err) {
      console.log(err);
      throw err;
    } finally {
      client.close();
    }

    return fn(null, data[0]);
  }

  async save(set, fn) {
    let client = null;
    let result = null;
    try {
      client = await MongoClient.connect(connectionString);
      const db = client.db(database);
      result = await db.collection('migrations').updateOne(
        {},
        {
          $set: {
            lastRun: set.lastRun,
          },
          $push: {
            migrations: { $each: set.migrations },
          },
        },
        { upsert: true }
      );
    } catch (err) {
      console.log(err);
      throw err;
    } finally {
      client.close();
    }

    return fn(null, result);
  }
}

module.exports = MongoDbStore;
