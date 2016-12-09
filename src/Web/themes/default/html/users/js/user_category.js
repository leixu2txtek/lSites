define(['../../../js/common'], function () {


    document.title = '用户栏目管理 - CMS内容管理系统';

    require(['template', 'moment', 'form', 'paging', 'ztree'], function (template, moment) {

        template.helper('format_date', function (date) {

            return moment(date).format('YYYY-MM-DD');
        });

        var nav = $('#nav_tools'),
            siteId = util.get_query('siteId'),
            userId = util.get_query('userId'),
            form = $('#user_category_form');

        $('#btn_add_user_category', nav).on('click', function () {

            var p_tree = $('<ul class="ztree" style="max-height: 275px; max-width: 280px; overflow: auto;"></ul>'),
                dlg = $M({
                    title: '选择栏目',
                    content: p_tree[0],
                    lock: true,
                    width: '300px',
                    height: '300px',
                    position: '50% 50%',
                    button: [
                        {
                            name: '添加所有',
                            callback: function () {

                                if (!confirm('确认添加所有栏目至当前用户？')) return false;

                                $.post(config.host + 'user/add_category_user', {
                                    siteId: siteId,
                                    userId: userId,
                                    all: true
                                }, function (r) {

                                    r = handleException(r);

                                    if (!r) return false;
                                    if (r.code < 0) {

                                        alert(r.msg || '发生未知错误，请刷新后尝试');
                                        return false;
                                    }

                                    dlg.close();

                                    $('[name=page]', form).val(1);
                                    form.submit();

                                }, 'json');

                                return false;
                            },
                            focus: true
                        },
                        {
                            name: '添加选中',
                            callback: function () {

                                var nodes = p_tree.getCheckedNodes(),
                                    categoryIds = nodes.map(function (a) { return a.id; });

                                if (categoryIds.length == 0) {

                                    alert('请选择要添加的栏目信息');
                                    return false;
                                }

                                if (!confirm('确认添加选中栏目至当前用户？')) return false;

                                $.post(config.host + 'user/add_category_user', {
                                    siteId: siteId,
                                    userId: userId,
                                    categoryIds: categoryIds
                                }, function (r) {

                                    r = handleException(r);

                                    if (!r) return false;
                                    if (r.code < 0) {

                                        alert(r.msg || '发生未知错误，请刷新后尝试');
                                        return false;
                                    }

                                    dlg.close();

                                    $('[name=page]', form).val(1);
                                    form.submit();

                                }, 'json');

                                return false;
                            }
                        }]
                });

            p_tree = $.fn.zTree.init(p_tree, {
                check: {
                    enable: true
                },
                async: {
                    enable: true,
                    url: config.host + 'category/list',
                    autoParam: ['id=parentId'],
                    otherParam: {
                        'siteId': siteId
                    },
                    type: "post"
                }
            });

            return false;
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + siteId + '" />');
            form.append('<input type="hidden" name="userId" value="' + userId + '" />');
        }

        form.gform({
            url: config.host + 'user/category_list',
            onSuccess: function (r) {
                r = handleException(r);

                if (!r) return false;
                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                $('#current_user').text(r.user.display_name);

                //更新总数
                $('#total_count', nav).html(r.paging.total_count);
                $('#table_container', form).html(template('user_category_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                //删除用户与栏目关系
                $('.delete', table).on('click', function () {

                    if (!confirm('是否确认将“该用户与栏目的关系”删除？')) return false;

                    var id = $(this).data('id');

                    $.post(config.host + 'user/delete_category_user', {
                        id: id,
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('已成功将该“用户与栏目关系”删除');
                        form.submit();

                    }, 'json');

                    return false;
                });

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) {
                form.submit();
            }
        });
    });
});