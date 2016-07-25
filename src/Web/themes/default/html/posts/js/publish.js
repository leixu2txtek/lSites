define(['../../../js/common'], function () {

    document.title = '已发布文章 - CMS内容管理系统';

    require(['template', 'ztree', 'form', 'select2'], function (template) {

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
                onClick: function (event, tId, node) {

                }
            }
        });

        var list = function (category) {

            

        };
    });
});