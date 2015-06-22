var BECon,
    contextMenuOpen = false;

function webSocketUrl() {
    return "ws://{0}:{1}/rcon".format(window.location.hostname, parseInt(window.location.port) + 1);
    //return "ws://localhost:8900/rcon";
}

function InputModal(t, p, c) {
    var modal = $('#InputModal'),
        form = modal.find('form'),
        title = modal.find('.modal-title'),
        input = form.find('.modal-body input');
    $('.modal').modal('hide');
    title.text(t);
    input.val('');
    input.attr('placeholder', p || '');
    form.on('submit', function (e) {
        e.preventDefault();
        modal.modal('hide');
        c(input.val());
    });
    modal.on('hidden.bs.modal', function (e) {
        modal.off();
        form.off();
    });
    modal.modal('show');
}

function BanInputModal(c) {
    var modal = $('#BanModal'),
        form = modal.find('form');
    form.on('submit', function (e) {
        e.preventDefault();
        var cmd = "addBan {0} {1} {2}",
            data = form.serializeObject();
        modal.modal('hide');
        c(data);
        modal.modal('hide');
    });
    modal.on('hidden.bs.modal', function (e) {
        modal.off();
        form.off();
    });
    modal.modal('show');
}

jQuery(function ($) {
    $('#WaitingModal').modal('show');
    BECon = MBConSocket(webSocketUrl());
    var DateFormat = 'DD/MM/YYYY HH:mm';

    $('.datetimepicker').datetimepicker({
        format: DateFormat,
        minDate: moment()
    }).attr('disabled', 'disabled');

    $('#LoginForm').on('submit', function (e) {
        e.preventDefault();
        var passwordField = $(this).find('input[type=password]'),
            password = passwordField.val();
        passwordField.val('');
        if (password == '') {
            alert('Password can not be blank.');
            return;
        }
        BECon.sendLogin(password);
    });

    $('#chat-form').on('submit', function (e) {
        e.preventDefault();
        var field = $('#message-input');
        var command = "say -1 {0}".format(field.val());
        field.val('');
        BECon.sendCommand(command);
    });

    // Nav commands
    $(".navbar-nav .setmaxping").on('click', function (e) {
        e.preventDefault();
        var id = $(this).parent().parent().parent().data('id');
        InputModal('Set Max Ping', 'Enter max ping', function (s) {
            var command = "MaxPing {0}".format(s);
            BECon.sendCommand(command);
        });
    });

    $(".navbar-nav .lock").on('click', function (e) {
        e.preventDefault();
        var command = "#lock";
        BECon.sendCommand(command);
    });

    $(".navbar-nav .unlock").on('click', function (e) {
        e.preventDefault();
        var command = "#unlock";
        BECon.sendCommand(command);
    });

    $(".navbar-nav .reloadscripts").on('click', function (e) {
        e.preventDefault();
        var command = "loadScripts";
        BECon.sendCommand(command);
    });

    $(".navbar-nav .reloadevents").on('click', function (e) {
        e.preventDefault();
        var command = "loadEvents";
        BECon.sendCommand(command);
    });

    //Modal stuff
    $('#ClientListModel').on('shown.bs.modal', function () {
        BECon.getClientList();
    });

    $('#BanListModel').on('shown.bs.modal', function () {
        BECon.getBanList();
    });

    $('#BanListModelTable').on('click', '.deleteban', function () {
        var row = BECon.table().row($(this).parents('tr'));
        row.data().delete(BECon);
    });

    $('#BanForm input[type=radio]').on('click', function (e) {
        if ($(this).val() == "-1") {
            $('.datetimepicker').attr('disabled', 'disabled');
        } else {
            $('.datetimepicker').removeAttr('disabled');
        }
    });

    // Right click stuff
    $('#player-container').on('contextmenu', 'tr', function (e) {
        $("#context-menu").data('id', $(this).data('id'));
        $("#context-menu").data('ip', $(this).data('ip'));
        $("#context-menu").data('guid', $(this).data('guid'));
        $("#context-menu").css({
            display: "block",
            left: e.pageX,
            top: e.pageY
        });
        contextMenuOpen = true;
        e.preventDefault();
        console.log($(this).data('id'));
    });

    $(document).on('click', function (e) {
        if (contextMenuOpen) {
            e.preventDefault();
            contextMenuOpen = false;
            $("#context-menu").css({
                display: "none",
                left: 0,
                top: 0
            });
        }
    });

    $("#context-menu .kick").on('click', function (e) {
        e.preventDefault();
        var id = $(this).parent().parent().parent().data('id');
        InputModal('Kick Message', 'Enter message', function (s) {
            if (s == '') {
                Reason
                alert('Message can not be blank.');
                return;
            }
            var command = "kick {0} {1}".format(id, s);
            BECon.sendCommand(command);
        }, 'Enter kick message');
    });

    $("#context-menu .ban").on('click', function (e) {
        e.preventDefault();
        var menu = $(this).parent().parent().parent(),
            id = menu.data('id'),
            ip = menu.data('ip'),
            guid = menu.data('guid');

        BanInputModal(function (data) {
            console.log(data);
            if (data['reason'] == '') {
                alert('Reason can not be blank.');
                return;
            }
            if (data['banlength'] == '1' && data['banlengthtime'] == '') {
                alert('Length can not be blank.');
                return;
            }
            var time = 0;
            if (data['banlength'] == '1') {
                var banMoment = moment(data['banlengthtime'], DateFormat);
                time = banMoment.diff(moment(), 'seconds');
                if (time < 1) {
                    alert('Ban length must be in the future!');
                    return;
                }
            }
            var cmd = 'addBan {0} {1} {2}';
            if (!(data['ban[]'] instanceof Array))
                data['ban[]'] = [data['ban[]']];
            var cmdArr = [];
            if ($.inArray('guid', data['ban[]']) > -1) {
                var banGuid = cmd.format(guid, time, data['reason']);
                cmdArr.push(banGuid);
            }
            if ($.inArray('ip', data['ban[]']) > -1) {
                var banIp = cmd.format(ip, time, data['reason']);
                cmdArr.push(banIp);
            }
            BECon.sendCommand(cmdArr);
        });
    });

    $("#context-menu .message").on('click', function (e) {
        e.preventDefault();
        var id = $(this).parent().parent().parent().data('id');
        InputModal('Send Message', 'Enter message', function (s) {
            if (s == '') {
                alert('Message can not be blank.');
                return;
            }
            var command = "say {0} {1}".format(id, s);
            BECon.sendCommand(command);
        });
    });
})
;