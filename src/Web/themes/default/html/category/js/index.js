define(['../../../js/common'], function () {

    document.title = '栏目管理 - CMS内容管理系统';

    require(['template', 'ztree', 'form'], function (template) {

        var nav = $('#nav_tools');

        $('.add_category', nav).on('click', function () {

            var form = $(template('category_form', { item: { site_id: util.get_query('siteId'), parent: {} } }));

            form.gform({
                url: config.host + 'category/save',
                onSuccess: function (r) {

                    if (!r || r.code < 0) {
                        alert(r.msg || '发生未知错误，请刷新尝试');
                        return false;
                    }

                    alert('保存成功');

                    callback && callback.apply(null, []);
                }
            });

            $('#edit_container').html(form);
        });

        //init ztree
        var tree = $.fn.zTree.init($("#category_tree"), {
            async: {
                enable: true,
                url: config.host + 'category/list',
                autoParam: ['id=parentId'],
                otherParam: { 'siteId': util.get_query('siteId') },
                type: "post"
            },
            callback: {
                onClick: function (event, treeId, treeNode) {

                    get_detail(treeNode.id, function () {

                        //refresh tree
                    });
                }
            }
        });

        //绑定点击ztree后事件
        var get_detail = function (id, callback) {

            $.post(config.host + 'category/detail', { id: id, siteId: util.get_query('siteId') }, function (r) {

                if (!r || r.code < 0) {
                    alert(r.msg || '发生未知错误，请刷新尝试');
                    return false;
                }

                var form = $(template('category_form', { item: r.data }));

                form.gform({
                    url: config.host + 'category/save',
                    onSuccess: function (r) {

                        if (!r || r.code < 0) {
                            alert(r.msg || '发生未知错误，请刷新尝试');
                            return false;
                        }

                        alert('保存成功');

                        callback && callback.apply(null, []);
                    }
                });

                $('#edit_container').html(form);
            }, 'json');
        };
    });
});