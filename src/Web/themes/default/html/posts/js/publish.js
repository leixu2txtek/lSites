define(['../../../js/common'], function() {

    document.title = '已发布文章 - CMS内容管理系统';

    require(['template', 'moment', 'select2', 'form', 'paging', 'ztree'], function(template, moment) {


        template.helper('format_date', function(date) {
            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#post_form');

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        }

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
                onClick: function(event, tId, node) {

                    $('[name=category]', form).val(node.id);

                    form.submit();
                }
            }
        });


        // TODO 绑定表单事件

        form.gform({
            url: config.host + 'posts/publish_list',
            onSuccess: function(r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('post_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);

            },
            callback: function(form) { form.submit(); }

        });
    });
});