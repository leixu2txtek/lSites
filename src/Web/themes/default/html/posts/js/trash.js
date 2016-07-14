define(['../../../js/common'], function () {

    document.title = '回收站 - 站群管理';

    require(['template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        var form = $('#trash_form');

        template.helper('format_date', function (date) {
            return moment(date).format('YYYY-MM-DD');
        });

        // select2
        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        }

        // 绑定表单
        form.gform({
            url: config.host + 'posts/trash',
            onSuccess: function (r) {
                if (!r || r.code < 0) {
                    alert(r.msg || '发生未知错误，请刷新尝试');
                    return false;
                }

                // 更新
                $('#trash_count').html(r.paging.total_count);
                $('#trashtable_container', form).html(template('trash_table', r));

                // 绑定表格
                var table = $('table', form).gtable();

                // 发布
                $('.publish', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定发布该文章')) return false;

                    $.post(config.host + 'posts/publish', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        if (!r || r.cod < 0) {
                            alert(r.msg || '发生未知错误，请刷新页面后尝试');
                            return false;
                        }

                        alert('发布成功');
                        form.submit();

                    }, 'json');

                    return false;
                });

                // 彻底删除
                $('.remove', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定彻底删除此文章')) return false;

                    $.post(config.host + 'posts/delete_completely', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        if (!r || r.code < 0) {
                            alert(r.msg || '发生未知错误，请刷新页面后尝试');
                            return false;
                        }

                        alert('文章已彻底删除');
                        form.submit();

                    }, 'json');

                    return false;
                });

                // 绑定分页
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) {
                form.submit();
            }
        });
    });
});