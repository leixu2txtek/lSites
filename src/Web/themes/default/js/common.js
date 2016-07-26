requirejs.config({
    baseUrl: '../../js/vendor',
    shim: {
        'ztree': ['jquery'],
        'select2': ['jquery'],
        'form': ['jquery'],
        'paging': ['jquery'],
        'pace': ['jquery'],
        'umeditor.config': ['jquery'],
        'umeditor': ['jquery']
    },
    paths: {
        'ztree': 'ztree/ztree',
        'select2': 'select2/select2',
        'form': 'jquery.form',
        'paging': 'jquery.paging',
        'template': 'template',
        'pace': 'pace',
        'MDialog': 'Mdialog/MDialog',
        'umeditor.config': 'umeditor/umeditor.config',
        'umeditor': 'umeditor/umeditor'
    }
});

const config = {
    host: '/',
    prefix: '/sites/html/',
};

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

const menus = [{
    title: '文章',
    icon: 'group-icon-207',
    key: 'posts',
    children: [{
        title: '我创建的',
        url: config.prefix + 'posts/index.html?siteId=' + util.get_query('siteId'),
        key: 'posts/index'
    }, {
            title: '待审核的',
            url: config.prefix + 'posts/audit.html?siteId=' + util.get_query('siteId'),
            key: 'posts/audit'
        }, {
            title: '已发布的',
            url: config.prefix + 'posts/publish.html?siteId=' + util.get_query('siteId'),
            key: 'posts/publish'
        }, {
            title: '回收站',
            url: config.prefix + 'posts/trash.html?siteId=' + util.get_query('siteId'),
            key: 'posts/trash'
        }]
}, {
        title: '栏目',
        icon: 'group-icon-nav',
        key: 'category',
        url: config.prefix + 'category/index.html?siteId=' + util.get_query('siteId'),
    }, {
        title: '选项',
        icon: 'group-icon-androidoptions',
        key: 'settings',
        children: [{
            title: '站点信息',
            url: config.prefix + 'settings/index.html?siteId=' + util.get_query('siteId'),
            key: 'settings/index'
        }, {
                title: '挂件管理',
                url: config.prefix + 'settings/widgets.html?siteId=' + util.get_query('siteId'),
                key: 'settings/widgets'
            }, {
                title: '参数配置',
                url: config.prefix + 'settings/config.html?siteId=' + util.get_query('siteId'),
                key: 'settings/config'
            }]
    }, {
        title: '用户管理',
        icon: 'group-icon-yonghu',
        key: 'users/index',
        url: config.prefix + 'users/index.html?siteId=' + util.get_query('siteId'),
    }, {
        title: '站点管理',
        icon: 'group-icon-zhandianguanli',
        key: 'sites/index',
        url: config.prefix + 'sites/index.html?siteId=' + util.get_query('siteId'),
    }];

const handleException = function (r) {

    if (!r) {

        alert('发生未知错误，请稍后再次尝试');
        return false;
    }

    if (r.code == 500 || r.code == 201 || r.code == 202) {
        alert(r.msg || '发生未知错误，请稍后再次尝试');
        return r;
    }

    if (r.code == 403) {
        alert('请先登录，再尝试访问该页面');

        window.location = '/users/login?returnUrl=' + window.location.href;
        return false;
    };

    return r;
};

define(['jquery', 'pace'], function ($) {

    //统一定义加载动画
    Pace.start();

    //加载成功后显示主体内容    
    Pace.on('hide', function () {
        $('#content_container').show();
    });

    var menu = $('<ul class="nav nav-stacked group-nav-sidebar"></ul>'),
        path = window.location.pathname;

    $.each(menus, function (i, v) {

        var li = $(['<li>',
            '<a href="' + (v.url || 'javascript:void(0);') + '" class="inactive">',
            '<i class="iconfont group-icon ' + v.icon + '"></i>' + v.title + '</a>',
            '</li>'
        ].join(''));

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