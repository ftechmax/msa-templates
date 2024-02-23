'use strict';
const MongoClient = require('mongodb').MongoClient;
const database = require('./../db').database;
const connectionString = process.env.CONNECTION_STRING;

module.exports.description = 'Added PaddedVersion field';

module.exports.up = async (next) => {
  const client = new MongoClient(connectionString);
  const db = client.db(database);

  let collections = ['EngineeringTypeMappingDocuments', 'FunctionToPortMappingDocuments', 'UserFunctionBlockLibraryDocuments'];

  let bulkWrites = [];
  for (let i = 0; i < collections.length; i++) {
    console.log(`processing ${collections[i]}`);
    const collection = db.collection(collections[i]);
    await collection.find({ PaddedVersion: { $exists: false } }).forEach((result) => {
      let versionArr = result.Version.split('.');
      let updateDocument = {
        updateOne: {
          filter: { _id: result._id },
          update: {
            $set: {
              PaddedVersion: `${('000' + versionArr[0]).slice(-3)}.${('000' + versionArr[1]).slice(-3)}.${('000' + versionArr[2]).slice(-3)}.${('000' + versionArr[3]).slice(-3)}`,
            },
          },
        },
      };
      bulkWrites.push(updateDocument);
    });

    if (bulkWrites.length > 0) {
      await collection.bulkWrite(bulkWrites);
    }
  }

  await client.close();
  next();
};

module.exports.down = (next) => {
  next();
};
