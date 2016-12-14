requirejs.config({
    baseUrl: '../../js/vendor',
    paths: {
        'ztree': 'ztree/ztree',
        'select2': 'select2/select2',
        'select2.cn': 'select2/zh-CN',
        'form': 'jquery.form',
        'paging': 'jquery.paging',
        'template': 'template',
        'pace': 'pace',
        'MDialog': 'Mdialog/MDialog',
        'ue': 'ueditor/ueditor',
        'ue.file': 'ueditor/dialogs/file/index',
        'ue.video': 'ueditor/dialogs/video/index'
    },
    shim: {
        'ztree': ['jquery'],
        'form': ['jquery'],
        'paging': ['jquery'],
        'pace': ['jquery'],
        'ue': ['ueditor/ueditor.config'],
        'ue.file': ['ue'],
        'ue.video': ['ue']
    }
});

const config = {
    host: '/',
    prefix: '/themes/default/html/',
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

const menus = {
    '管理员': [{
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
    }],
    '审核人': [{
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
    }],
    '编辑': [{
        title: '文章',
        icon: 'group-icon-207',
        key: 'posts',
        children: [{
            title: '我创建的',
            url: config.prefix + 'posts/index.html?siteId=' + util.get_query('siteId'),
            key: 'posts/index'
        }, {
            title: '回收站',
            url: config.prefix + 'posts/trash.html?siteId=' + util.get_query('siteId'),
            key: 'posts/trash'
        }]
    }]
};

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
        alert('没有权限访问该页面');

        window.location = '/users/login?returnUrl=' + window.location.href;
        return false;
    };

    return r;
};

define(['jquery', 'template', 'pace', 'MDialog'], function ($, template) {

    $.extend({
        _hasFile: function (tag, url) {
            var contains = false;
            var type = (tag == "script") ? "src" : "href";

            $(tag + '[' + type + ']').each(function (i, v) {
                var attr = $(v).attr(type)
                if (attr == url || decodeURIComponent(attr).indexOf(url) != -1) {
                    contains = true;
                    return false;
                }
            });

            return contains;
        },
        _loadCss: function (href, cb) {

            if (!$._hasFile('link', href)) {

                var ele = document.createElement("link");
                ele.setAttribute('type', 'text/css');
                ele.setAttribute('rel', 'stylesheet');
                ele.setAttribute('href', href);

                document.getElementsByTagName('head')[0].appendChild(ele);

                ele.onload = function () {
                    if (cb && $.isFunction(cb)) cb.apply(this, []);
                };
            } else {

                if (cb && $.isFunction(cb)) cb.apply(this, []);
            }
        }
    });

    //统一定义加载动画
    Pace.start();

    //加载成功后显示主体内容    
    Pace.on('hide', function () {
        $('.content_container').show();
    });

    //用户信息
    $.ajax({
        url: config.host + 'open/get_user_info',
        type: 'POST',
        dataType: 'json',
        data: {
            siteId: util.get_query('siteId')
        },
        async: false
    }).done(function (r) {

        if (!r || r.code < 0) {
            alert(r.msg || '发生未知错误，请刷新页面后尝试');

            history.go(-1);
            return false;
        }

        var menu = $('<ul class="nav nav-stacked group-nav-sidebar"></ul>'),
            path = window.location.pathname;

        $.each(menus[r.current.role], function (i, v) {

            var li = $(['<li>', '<a href="' + (v.url || 'javascript:void(0);') + '" class="inactive">', '<i class="iconfont group-icon ' + v.icon + '"></i>' + v.title + '</a>', '</li>'].join(''));

            //active current menu        
            if (path.indexOf(v.key) != -1) $('a', li).addClass('active');

            if (v.children && v.children.length > 0) {

                li.find('a:first').append('<i class="iconfont group-icon-add sub-add"></i>');

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

        //追加菜单
        $('.sidebar').append(menu);

        //设置用户信息
        var info = $(template.compile('<div class="navbar-collapse collapsc navbar-right group-top-userbox"><span>欢迎您，{{display_name}}</span> <a href="javascript:(0);" class="group-top-user user_info"><img src="{{avatar}}" class="img-circle" width="38px" height="38px"> <i class="iconfont group-icon-usersmore"></i></a><div class="nav group-user-dropdown" style="display:none"><ul><i class="group-user-dropdownIcon">&nbsp;</i><li><a href="#" title="个人资料" class="">个人资料</a></li><li><a href="#" title="修改密码" class="modifypwd" id="modify_pwd">修改密码</a></li><li><a href="/users/logout" title="退出">退出</a></li></ul></div></div>')({
            display_name: r.display_name,
            avatar: r.avatar || 'data:image/jpeg;base64,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'
        }));

        //用户信息
        $('.user_info', info).on('click', function () {

            var next = $(this).next(),
                status = next.data('open');

            status ? next.hide(300).data('open', false) : next.show(300).data('open', true);

            $('body').one('click', function () {
                next.hide(300).data('open', false);
            });

            return false;
        });

        //修改密码
        $('#modify_pwd', info).on('click', function () {

            var form = $(template.compile('<form class="form-horizontal password-form" ><div class="form-group"><label class="col-sm-2 control-label">原始密码：</label><div class="col-sm-10 password-input"><input type="password" class="form-control" name="oriPassword" placeholder="原始密码" ></div></div><div class="form-group"><label class="col-sm-2 control-label">新密码：</label><div class="col-sm-10 password-input"><input type="password" class="form-control" name="newPassword" placeholder="新密码" ></div></div><div class="form-group"><label class="col-sm-2 control-label">确认新密码：</label><div class="col-sm-10 password-input"><input type="password" class="form-control" name="newPassword2" placeholder="确认新密码" ></div></div></form>')({})),
                dlg = {};

            form.gform({
                url: config.host + 'open/update_password',
                beforeSubmit: function () {

                    var oriPassword = $('[name=oriPassword]', form).val(),
                        newPassword = $('[name=newPassword]', form).val(),
                        newPassword2 = $('[name=newPassword2]', form).val();

                    if (oriPassword.length == 0) {

                        $('[name=oriPassword]', form).parent().addClass('has-error');
                        alert('原始密码不能为空');

                        return false;
                    }

                    if (newPassword.length == 0) {

                        $('[name=newPassword]', form).parent().addClass('has-error');
                        alert('新密码不能为空');

                        return false;
                    }
                    if (newPassword2.length == 0) {

                        $('[name=newPassword2]', form).parent().addClass('has-error');
                        alert('确认密码不能为空');

                        return false;
                    }
                    if (newPassword.length < 6 || newPassword.length > 18) {

                        alert('密码长度只能是6 ~ 18位');
                        $('input[name=newPassword]').focus();

                        return false;
                    }

                    if (/\s/g.test(newPassword)) {

                        alert('密码中不能包含空格');
                        $('input[name=newPassword]').focus();

                        return false;
                    }

                    if (newPassword != newPassword2) {

                        alert('确认密码与新密码不符，请重新输入');
                        $('input[name=newPassword2]').focus();

                        return false;
                    }
                },
                onSuccess: function (r) {

                    r = handleException(r);

                    if (!r) return false;
                    if (r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    alert(r.msg);

                    dlg.close();
                }
            });

            $._loadCss(config.host + 'themes/default/js/vendor/Mdialog/MDialog.css', function () {

                dlg = $M({
                    title: '修改密码',
                    content: form[0],
                    lock: true,
                    width: '400px',
                    height: '210px',
                    position: '50% 50%',
                    ok: function () {

                        form.submit();
                    },
                    okVal: '保存',
                    cancel: false,
                    cancelVal: '取消'
                });
            });

            return false;
        });

        $('#nav_header').append(info);

        //设置当前站点，以及用户所管理的站点
        var html = $(template.compile('<div class="select-site"><h2 class="select-act"><i class="iconfont group-icon group-icon-home"></i>{{current.title}}<em class="iconfont group-icon group-icon-jiantou"></em></h2><ul class="select-sitelist" style="display:none">{{each sites as item i}}<li><a href="' + config.host + 'themes/default/html/posts/index.html?siteId={{item.id}}" class="{{if item.id == current.id}}active{{/if}}">{{item.title}}</a></li>{{/each}}</ul></div>')({
            sites: r.sites,
            current: r.current
        }));

        //选择站点
        $('.select-act', html).on('click', function () {

            var _this = $(this);

            if (_this.data('open')) {

                _this.next().slideUp(200);
                _this.data('open', false);
                _this.find('em').removeClass('group-icon-shangla').addClass('group-icon-jiantou');
            } else {

                _this.next().slideDown(200);
                _this.data('open', true);
                _this.find('em').removeClass('group-icon-jiantou').addClass('group-icon-shangla');
            }

            $('body').one('click', function () {

                _this.next().slideUp(200);
                _this.data('open', false);
                _this.find('em').removeClass('group-icon-shangla').addClass('group-icon-jiantou');
            });

            return false;
        });

        $('.sidebar').append(html);
    });

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