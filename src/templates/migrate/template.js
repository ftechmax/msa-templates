'use strict';
const MongoClient = require('mongodb').MongoClient;
const database = require('./../db').database;
const connectionString = process.env.CONNECTION_STRING;

module.exports.description = 'MY_DESCRIPTION';

module.exports.up = async (next) => {
  const client = new MongoClient(connectionString);
  const db = client.db(database);
  const collection = db.collection('MY_COLLECTION');

  let bulkWrites = [];

  if (bulkWrites.length > 0) {
    await collection.bulkWrite(bulkWrites);
  }

  await client.close();
  next();
};

module.exports.down = (next) => {
  next();
};
