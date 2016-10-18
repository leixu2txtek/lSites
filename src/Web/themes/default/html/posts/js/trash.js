define(['../../../js/common'], function () {

    document.title = '回收站 - CMS内容管理系统';

    require(['template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        var form = $('#trash_form'),
            nav = $('#nav_tools');

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

        // 批量删除
        $('.btn_empty', nav).on('click', function () {

            var siteId = util.get_query('siteId'),
                ids = $('table:first', form).get_selected_row_id();
            if (ids.length == 0) {
                if (!confirm('确定清空回收站？')) return false;
            } else {
                if (!confirm('是否将选中的文章彻底删除？')) return false;
            }

            var p_delete = function (confirmed) {
                $.post(config.host + 'posts/delete_completely', {
                    ids: ids,
                    siteId: siteId,
                    confirmed: confirmed || false
                }, function (r) {

                    r = handleException(r);

                    if (!r) return false;
                    if (r.code < 0) {

                        alert(r.msg || '发生未知错误，请刷新后尝试');
                        return false;
                    }

                    if (r.code == 1) {

                        alert('已成功将文章移至回收站');
                        form.submit();

                        return false;
                    }

                    confirm('选中的文章已经发布，是否确认要移至回收站？') && p_delete(true);
                }, 'json');

            };

            p_delete(false);

            return false;
        });

        // 绑定表单
        form.gform({

            url: config.host + 'posts/trash',
            onSuccess: function (r) {

                r = handleException(r);

                if (!r) return false;
                if (r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                // 更新
                $('#trash_count').html(r.paging.total_count);
                $('#trashtable_container', form).html(template('trash_table', r));

                // 绑定表格
                var table = $('table', form).gtable();

                table.on('gtable.checked', function (event, ids) {

                    $('.btn-danger').html(ids.length == 0 ? '清空回收站' : '批量删除');
                });

                // 发布
                $('.publish', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定发布该文章')) return false;

                    $.post(config.host + 'posts/publish', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('发布成功');
                        form.submit();

                    }, 'json');

                    return false;
                });

                // 彻底删除
                $('.remove', table).on('click', function () {

                    r = handleException(r);

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确定彻底删除此文章')) return false;

                    $.post(config.host + 'posts/delete_completely', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('文章已彻底删除');
                        form.submit();

                    }, 'json');

                    return false;
                });

                // 恢复回收站的文章
                $('.q-restore', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否撤销删除的文章？')) return false;

                    $.post(config.host + 'posts/restore', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r) return false;
                        if (r.code < 0) {

                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('撤销成功');
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