'use strict';
var migrate = require('migrate');
const MongoDbStore = require('./mongodb-store');

migrate.load(
  {
    stateStore: new MongoDbStore(),
  },
  (err, set) => {
    if (err) {
      throw err;
    }
    set.up(function (err) {
      if (err) {
        throw err;
      }
      console.log('migrations successfully ran');
    });
  }
);
