const { v4: uuidv4 } = require('uuid');

function main(args) {
  return {
    body: uuidv4()
  }
}

exports.main = main

