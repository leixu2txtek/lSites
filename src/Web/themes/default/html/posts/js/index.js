define(['../../../js/common'], function () {

    document.title = '我创建的文章 - CMS内容管理系统';

    require(['template', 'moment', 'select2', 'form', 'paging'], function (template, moment) {

        template.helper('format_date', function (date) {
            return moment(date).format('YYYY-MM-DD');
        });

        var form = $('#post_form'),
            nav = $('#nav_tools');

        $('#btn_add', nav).prop('href', 'edit.html?siteId=' + util.get_query('siteId')); 

        //批量发布        
        $('.btn_publish', nav).on('click', function () {

            var siteId = util.get_query('siteId'),
                ids = $('table:first', form).get_selected_row_id();

            if (ids.length == 0) {

                alert('请选择要发布的文章');
                return false;
            }

            if (!confirm('是否确认发布选中的文章？')) return false;

            $.post(config.host + 'posts/publish', {
                siteId: siteId,
                ids: ids
            }, function (r) {

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                alert('发布成功');
                form.submit();

            }, 'json');

            return false;
        });

        //批量放入回收站      
        $('.btn_delete', nav).on('click', function () {

            var siteId = util.get_query('siteId'),   
                ids = $('table:first', form).get_selected_row_id(); 

            if (ids.length == 0) {

                alert('请选择要放入回收站的文章');
                return false;
            }

            if (!confirm('是否确认将选中的文章放入回收站？')) return false;

            var p_delete = function (confirmed) {

                $.post(config.host + 'posts/delete', {
                    ids: ids,
                    siteId: siteId,
                    confirmed: confirmed || false
                }, function (r) {

                    r = handleException(r);

                    if (!r || r.code < 0) {
                        alert(r.msg || '发生未知错误，请刷新页面后尝试');
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

        $('select', form).select2({
            minimumResultsForSearch: -1,
            allowClear: true
        });

        if (form.find('input[name=siteId]').length == 0) {
            form.append('<input type="hidden" name="siteId" value="' + util.get_query('siteId') + '" />');
        }

        //绑定表单
        form.gform({
            url: config.host + 'posts/list',
            onSuccess: function (r) {

                r = handleException(r);

                if (!r || r.code < 0) {

                    alert(r.msg || '发生未知错误，请刷新后尝试');
                    return false;
                }

                //更新总数
                $('#total_count').html(r.paging.total_count);
                $('#table_container', form).html(template('post_table', r));

                //绑定表格                
                var table = $('table', form).gtable();

                //发布
                $('.publish', table).on('click', function () {
                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确认发布该文章？')) return false;

                    $.post(config.host + 'posts/publish', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r || r.code < 0) {
                            alert(r.msg || '发生未知错误，请刷新页面后尝试');
                            return false;
                        }

                        alert('发布成功');
                        form.submit();

                    }, 'json');

                    return false;
                });

                //移至回收站                
                $('.delete', table).on('click', function () {

                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否确认将该文章移至回收站？')) return false;

                    var p_delete = function (confirmed) {

                        $.post(config.host + 'posts/delete', {
                            ids: [id],
                            siteId: siteId,
                            confirmed: confirmed || false
                        }, function (r) {

                            r = handleException(r);

                            if (!r || r.code < 0) {
                                alert(r.msg || '发生未知错误，请刷新页面后尝试');
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


                // 取消发布

                $('.unpublished', table).on('click', function () {
                    var siteId = util.get_query('siteId'),
                        id = $(this).data('id');

                    if (!confirm('是否取消发布该文章？')) return false;

                    $.post(config.host + 'posts/unpublish', {
                        ids: [id],
                        siteId: siteId
                    }, function (r) {

                        r = handleException(r);

                        if (!r || r.code < 0) {
                            alert(r.msg || '发生未知错误，请刷新后尝试');
                            return false;
                        }

                        alert('取消成功');
                        form.submit();

                    }, 'json');

                    return false;
                });

                //绑定分页信息                
                $('.x-paging-container', form).paging(r.paging);
            },
            callback: function (form) { form.submit(); }
        });
    });
});