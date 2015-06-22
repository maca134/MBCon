jQuery(function ($) {
    $('#resize-handle').on('mousedown', function (e) {
        $('body').css({
            'user-modify': 'none',
            '-moz-user-select': 'none',
            '-webkit-user-select': 'none',
            '-ms-user-select': 'none'
        });
        $(document).on('mousemove', function (e) {
            var playerWidth = ((e.pageX - 1) / $(window).width());
            var mainWidth = (1 - playerWidth);
            $('#player-container').width(playerWidth * 100 + '%');
            $('#main-container').width(mainWidth * 100 + '%');
        }).on('mouseup', function (e) {
            $(document).off('mousemove').off('mouseup');
            $('body').removeAttr('style');
        });
    });
});