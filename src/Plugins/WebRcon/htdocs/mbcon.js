function PlayerBan(id, data, time, reason) {
    this.id = id;
    this.data = data;
    this.time = time;
    this.reason = reason;
    this.actions = function () {
        return '<a href="#" class="btn btn-sm btn-danger deleteban" data-id="{0}">Delete</a>'.format(this.id);
    }
    this.delete = function (MBCon) {
        if (!confirm('Are you sure you want to delete this ban?'))
            return;
        var cmd = 'removeBan {0}'.format(this.id);
        MBCon.sendCommand(cmd);
    };
}

function MBConSocket(wsurl) {
    var ws = new ReconnectingWebSocket(wsurl, null, { debug: false, reconnectInterval: 4000 }),
        wsOpen = false,
        authToken = false,
        banList = [],
        banListTable,
        logoutput = $('#log-output'),
        maincontainer = $('#main-container');

    function LogMessage(bemessage) {
        var type = bemessage.msgtype.toLowerCase(),
            message = bemessage.message,
            liclass = "message-server message-{0}".format(type);
        switch (type) {
            case "chat":
                var match = /\(([^\)]+)\) /.exec(message);
                liclass = "{0} message-{1}".format(liclass, match[1].toLowerCase());
                message = message.replace(match[0], '<span class="type">{0}</span> '.format(match[1]));
                break;
            case "kick":
                message = '<span class="type">Kick</span>' + message;
                break;
            case "disconnect":
                message = '<span class="type">Disconnect</span>' + message;
                break;
            case "connectip":
            case "connectguid":
            case "connectlegacyguid":
                message = '<span class="type">Connect</span>' + message;
                break;
            default:
                message = '<span class="type">Log</span>' + message;
                break;
        }
        logoutput.append('<li class="{0}">{1}</li>'.format(liclass, message));

        maincontainer.scrollTop(maincontainer[0].scrollHeight);
    }

    function UpdatePlayers(players) {
        var template = Handlebars.compile($('#player-template').html());
        $('#player-list tbody').empty();
        $.each(players, function (i) {
            $('#player-list tbody').append(template(this));
        });
    }

    function LoadBansTable(players) {
        if (banList.length == 0) {
            $.each(players, function (i) {
                banList.push(new PlayerBan(
                    this.id,
                    this.data,
                    this.time,
                    this.reason
                ));
            });
            banListTable = $('#BanListModelTable').DataTable({
                data: banList,
                columns: [
                    { data: 'data' },
                    { data: 'time' },
                    { data: 'reason' },
                    {
                        data: 'actions',
                        orderable: false
                    }
                ]
            });
        }
    }

    function LoadClientsTable(clients) {
        var table = $('#ClientListModelTable tbody');
        table.empty();
        $.each(clients, function (i) {
            table.append('<tr><td>{0}</td><td>{1}</td></tr>'.format(this['ClientId'], this['Ip']));
        });
    }

    ws.onopen = function () {
        console.log("MBCon Socket Open");
        wsOpen = true;
        $('.modal').modal('hide');
        $('#LoginModal').modal('show');
        $('#LoginModal').find('input[type=password]').first().focus();
    };
    ws.onmessage = function (evt) {
        var data = JSON.parse(evt.data);
        switch (data['type']) {
            case -1: // Error
                alert(data['error']);
                break
            case 0: //Login
                if (data['success'] == false) {
                    alert('Login failed.');
                    return;
                }
                $('.modal').modal('hide');
                break;
            case 1: // Token
                authToken = data['token'];
                console.log("New token sent: ", authToken);
                break;
            case 2: // Log
                LogMessage(data);
                break;
            case 3: // Players
                UpdatePlayers(data['players'])
                break;
            case 4: // Load ban list
                LoadBansTable(data['bans']);
                break
            case 5: // Load be client list
                LoadClientsTable(data['clients']);
                break
        }
    };
    ws.onclose = function () {
        console.log("MBCon Socket Closed");
        wsOpen = false;
        authToken = false;
        $('.modal').not('#WaitingModal').modal('hide');
        $('#WaitingModal').modal('show');
    };
    return {
        table: function () {
            return banListTable;
        },
        isConnected: function () {
            return wsOpen;
        },
        sendLogin: function (password) {
            var data = {
                type: 0,
                password: password
            };
            ws.send(JSON.stringify(data));
        },
        sendCommand: function (command) {
            if (!authToken) {
                alert('Not logged in, can not send command.');
            }
            if (!(command instanceof Array)) {
                command = [command];
            }
            var data = {
                type: 1,
                data: command,
                token: authToken
            };
            ws.send(JSON.stringify(data));
        },
        getBanList: function () {
            if (!authToken) {
                alert('Not logged in, can not send command.');
            }
            var data = {
                type: 4,
                token: authToken
            };
            ws.send(JSON.stringify(data));
        },
        getClientList: function () {
            if (!authToken) {
                alert('Not logged in, can not send command.');
            }
            var data = {
                type: 5,
                token: authToken
            };
            ws.send(JSON.stringify(data));
        }
    };
}