$(function() {
    $(".group-top-user").click(function() {
        $(".group-user-dropdown").toggle();
    });
    // 头部下拉框

    $('.inactive').click(function() {
        if ($(this).siblings('ul').css('display') == 'none') {
            $(this).parent('li').siblings('li').removeClass('active');
            $(this).addClass('active');
            $(this).siblings('ul').slideDown(600).children('li');
            var _child = $(this).parents('li').siblings('li').children('ul');

            if (_child.css('display') == 'block') {
                _child.parent('li').children('a').removeClass('active');
                _child.slideUp(600);

            }
        } else {
            var _child = $(this).siblings('ul');
            //控制自身变成+号
            $(this).removeClass('active');
            //控制自身菜单下子菜单隐藏
            _child.slideUp(100);
            //控制自身子菜单变成+号
            _child.children('li').children('ul').parent('li').children('a').addClass('active');
            //控制自身菜单下子菜单隐藏
            _child.children('li').children('ul').slideUp(100);

            //控制同级菜单只保持一个是展开的（-号显示）
            _child.children('li').children('a').removeClass('active');
        }
    })

})