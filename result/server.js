var express = require('express'),
    async = require('async'),
    azure = require("azure-storage"),
    cookieParser = require('cookie-parser'),
    bodyParser = require('body-parser'),
    methodOverride = require('method-override'),
    app = express(),
    server = require('http').Server(app),
    io = require('socket.io')(server);

io.set('transports', ['polling']);

var port = process.env.PORT || 4000;
var storage_account = process.env.AZURE_STORAGE_ACCOUNT;
var storage_access_key = process.env.AZURE_STORAGE_ACCESS_KEY;

io.sockets.on('connection', function (socket) {
  socket.emit('message', { text : 'Welcome!' });
  socket.on('subscribe', function (data) {
    socket.join(data.channel);
  });
});

async.retry(
  {times: 1000, interval: 1000},
  function(callback) {
    var tableSvc = azure.createTableService(storage_account, storage_access_key);
    callback(null, tableSvc);
  },
  function(err, client) {
    getVotes(client);
  }
);

function getVotes(client) {
  var query = new azure.TableQuery().where('PartitionKey eq ?', 'votecount');
  client.queryEntities('votes', query, null, function(queryError, queryResult, queryResponse) {
      if (queryError) { 
          console.error(queryError);
      } else {
        var votes = {};
        for (var entity of queryResult.entries) {
          var vote = entity['RowKey']._;
          var count = entity['Count']._;
          votes[vote] = count;
        }
        io.sockets.emit("scores", JSON.stringify(votes));
      }

      setTimeout(function() {getVotes(client) }, 1000);
  });
}

app.use(cookieParser());
app.use(bodyParser());
app.use(methodOverride('X-HTTP-Method-Override'));
app.use(function(req, res, next) {
  res.header("Access-Control-Allow-Origin", "*");
  res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
  res.header("Access-Control-Allow-Methods", "PUT, GET, POST, DELETE, OPTIONS");
  next();
});

app.use(express.static(__dirname + '/views'));

app.get('/', function (req, res) {
  res.sendFile(path.resolve(__dirname + '/views/index.html'));
});

server.listen(port, function () {
  var port = server.address().port;
  console.log('App running on port ' + port);
});
