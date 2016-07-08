requirejs.config({
    baseUrl: '../../js/vendor',
    shim: {
        'ztree': ['jquery'],
        'bootstrap': ['jquery'],
        'select2': ['jquery'],
        'form': ['jquery'],
    },
    paths: {
        'ztree': 'ztree',
        'bootstrap': 'bootstrap.min',
        'select2': 'select2/select2.min',
        'form': 'jquery.form',
        'art-template': 'art-template'
    }
});

const config = {
    host: '/',
    prefix: '/sites/html/',
};

const menus = [
    {
        title: '文章',
        icon: 'group-icon-207',
        key: 'posts',
        children: [
            {
                title: '我创建的',
                url: config.prefix + 'posts/index.html',
                key: 'posts/index'
            },
            {
                title: '待审核的',
                url: config.prefix + 'posts/audit.html',
                key: 'posts/audit'
            },
            {
                title: '已发布的',
                url: config.prefix + 'posts/publish.html',
                key: 'posts/publish'
            },
            {
                title: '回收站',
                url: config.prefix + 'posts/trash.html',
                key: 'posts/trash'
            }
        ]
    },
    {
        title: '栏目',
        icon: 'group-icon-nav',
        key: 'category',
        url: config.prefix + 'category/index.html',
    },
    {
        title: '选项',
        icon: 'group-icon-androidoptions',
        key: 'settings',
        children: [
            {
                title: '站点信息',
                url: config.prefix + 'settings/index.html',
                key: 'settings/index'
            },
            {
                title: '挂件管理',
                url: config.prefix + 'settings/widgets.html',
                key: 'settings/widgets'
            },
            {
                title: '参数配置',
                url: config.prefix + 'settings/config.html',
                key: 'settings/config'
            }
        ]
    },
    {
        title: '用户管理',
        icon: 'group-icon-yonghu',
        key: 'users/index',
        url: config.prefix + 'users/index.html',
    },
    {
        title: '站点管理',
        icon: 'group-icon-zhandianguanli',
        key: 'sites/index',
        url: config.prefix + 'sites/index.html',
    },
];

const util = {
    get_query: function (key) {
        var arr = [],
            obj = {},
            location = window.location.href,
            has = location.indexOf('?') > -1,
            isHash = location.indexOf('#');

        if (!has) return null;
        arr = location.substring(location.indexOf('?') + 1, isHash > -1 ? isHash : location.length).split('&');

        for (var i = 0, len = arr.length; i < len; i++) {
            var temp = arr[i].split('=');
            if (temp.length) obj[temp[0]] = temp[1].replace(/#/g, '');
        }

        if (key) {
            return obj[key];
        } else {
            return obj;
        }
    }
}

define(['jquery'], function ($) {
    var menu = $('<ul class="nav nav-stacked group-nav-sidebar"></ul>'),
        path = window.location.pathname;

    $.each(menus, function (i, v) {

        var li = $(['<li>',
            '<a href="' + (v.url || 'javascript:void(0);') + '" class="inactive">',
            '<i class="iconfont group-icon ' + v.icon + '"></i>' + v.title + '</a>',
            '</li>'].join(''));

        //active current menu        
        if (path.indexOf(v.key) != -1) $('a', li).addClass('active');

        if (v.children && v.children.length > 0) {

            li.find('a:first').append('<i class="glyphicon glyphicon-plus group-nav-more"></i>');

            var subs = '<ul class="nav nav-stacked group-subMenu" style="display: none">';

            $.each(v.children, function (i, v) {

                if (path.indexOf(v.key) != -1) {
                    subs += '<li class="active"><a href="' + v.url + '">' + v.title + '</a></li>'
                } else {
                    subs += '<li><a href="' + v.url + '">' + v.title + '</a></li>'
                }
            });

            subs += '</ul>';

            li.append(subs);

            if (li.find('li.active').length != 0) li.find('ul:first').show();
        }

        menu.append(li);
    });

    $('.sidebar').append(menu);

    $('.inactive').click(function () {
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
    });
});