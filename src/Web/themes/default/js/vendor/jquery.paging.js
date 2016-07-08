(function ($) {

    $.fn.paging = function (opts) {
        opts = $.extend(true, {}, $.fn.paging.defaults, opts);

        if (opts.total_count == 0) return false;
        if (opts.page_size == 0) return false;

        //渲染HTML        
        var render_html = function () {

            var page_count = Math.ceil(opts.total_count / opts.page_size),
                container = $('<div class="x-paging"></div>'),
                left = '<div class="x-paging-left">共 <span style="height:20px;">{{total_count}}</span> 条，每页显示<select name="pageSize" style="width: 50px; margin: 0px 5px;"></select>条</div>',
                right = '<ul class="x-paging-right"></ul>';

            //render left
            if (opts.show_left_info) {

                left = left.replace('{{total_count}}', opts.total_count);
                left = $(left);

                var options = [10, 20, 50, 100],
                    temp = [];

                if (options.filter(function (a) { return a == opts.page_size; }).length == 0) {

                    temp = options.filter(function (a) { return a > opts.page_size; });

                    options.splice(temp.indexOf(temp[0]), 0, opts.page_size);
                }

                //构造选项HTML                
                temp = '';
                $.each(options, function (i, v) {
                    temp += '<option value="' + v + '" >' + v + '</option>';
                });

                $('select', left).append(temp);

                $('select', left).val(opts.page_size);

                container.append(left);
            }

            //do not need display the paging info
            if (opts.page_size >= opts.total_count) return container;

            //render right
            right = $(right);

            //start & end
            opts.show_start && right.append('<li class="x-paging-start" title="' + opts.start_txt + '"><a href="javascript:void(0);" data-page="1">' + opts.start_txt + '</a></li>');
            opts.show_end && right.append('<li class="x-paging-end" title="' + opts.end_txt + '"><a href="javascript:void(0);" data-page="' + page_count + '">' + opts.end_txt + '</a></li>');

            //prev & next
            var start = $('.x-paging-start', right),
                end = $('.x-paging-end', right),
                prev_page = Math.max(opts.page_index - 1, 1),
                next_page = Math.min(opts.page_index + 1, page_count),
                prev_ele = $('<li class="x-paging-prev" title="' + opts.prev_txt + '"><a href="javascript:void(0);" data-page="' + prev_page + '">' + opts.prev_txt + '</a></li>'),
                next_ele = $('<li class="x-paging-next" title="' + opts.next_txt + '"><a href="javascript:void(0);" data-page="' + next_page + '">' + opts.next_txt + '</a></li>');

            opts.page_index != 1 && opts.show_prev && (start.length == 1 ? start.after(prev_ele) : right.append(prev_ele));
            opts.page_index != page_count && opts.show_next && (end.length == 1 ? end.before(next_ele) : right.append(next_ele));

            //page
            prev_ele = $('.x-paging-prev', right);
            next_ele = $('.x-paging-next', right);

            var pages = '',
                display_count = 5,
                p = [];

            //总页数小于等于要显示的数量，则全部显示
            if (page_count <= display_count) {

                for (var i = 1; i <= page_count; i++) {

                    p.push({ type: 'item', value: i });
                }
            } else {

                //构造当前页码附近的页数，左边2个，右边2个
                for (var i = 2; i >= 0; i--) {

                    if (opts.page_index - i <= 0) continue;

                    p.push({ type: 'item', value: opts.page_index - i });
                }

                for (var i = 1; i <= (display_count - p.length + 1); i++) {

                    if (opts.page_index + i > page_count) continue;

                    p.push({ type: 'item', value: opts.page_index + i });
                }

                if (p[0].value > 1) {
                    p.splice(0, 0, { type: 'item', value: 1 });
                    p.splice(1, 0, { type: 'ellipsis' });
                }

                if (p[p.length - 1].value < page_count) {
                    p[p.length] = { type: 'ellipsis' };
                    p[p.length] = { type: 'item', value: page_count };
                }
            }

            //渲染生成后的页码信息        
            for (var i = 0; i < p.length; i++) {

                if (p[i].type == 'item' && p[i].value == opts.page_index) {

                    pages += '<li class="x-paging-active"><a href="javascript:void(0);" data-page="' + p[i].value + '">' + p[i].value + '</a></li>';
                    continue;
                }

                if (p[i].type == 'item') {

                    pages += '<li ><a href="javascript:void(0);" data-page="' + p[i].value + '">' + p[i].value + '</a></li>';
                    continue;
                }

                if (p[i].type == 'ellipsis') pages += '<li><span class="x-paging-ellipsis">…</span></li>';
            }

            pages = $(pages);

            if (next_ele.length == 1) next_ele.before(pages);
            if (next_ele.length == 0 && end.length == 1) end.before(pages);
            if (next_ele.length == 0 && end.length == 0) right.append(pages);

            return container.append(right);
        };

        var write_css = function () {

            if ($('style[data-type="x-paging"]').length == 1) return false;

            var css = ['.x-paging .x-paging-left { float: left; }',
                '.x-paging-right { float: right; display: block; margin: 0 auto; padding-left: 0; height: 34px; border-radius: 4px; line-height: 26px; }',
                '.x-paging-right li { display: inline; }',
                '.x-paging-right li a, .x-paging-right li span { position: relative; border: 1px solid #ddd; background-color: #fff; color: #337ab7; text-decoration: none; line-height: 1.42857143; }',
                '.x-paging-right li a:hover { background: #337ab7; color: #fff; }',
                '.x-paging-right li a { margin: 0px 1px; padding: 2px 12px; border-radius: 0px; }',
                '.x-paging-right li.x-paging-active a { background: #337ab7; color: #fff; }',
                '.x-paging-right .x-paging-ellipsis { margin: 0px 4px; border: 0px; }'].join('');

            $('head').append('<style type="text/css" data-type="x-paging">' + css + '</style>');
        } ();

        return $(this).each(function (i, v) {

            var container = render_html(),
                form = $(v).parents('form:first');

            if ($('[name=page]', form).length == 0) form.append('<input type="hidden" name="page" />');

            var c_page = $('[name=page]', form).val();

            $('li a', container).on('click', function () {

                if (!$(this).data('page')) return false;

                $('[name=page]', form).val($(this).data('page'));
                form.submit();
            });

            $('select', container).on('change', function () {

                form.submit();
            });

            $(this).replaceWith(container);
        });
    };

    $.fn.paging.defaults = {
        total_count: 0,         //总数
        page_index: 1,          //当前页数
        page_size: 10,          //分页大小
        show_prev: true,        //显示上一页
        show_next: true,        //显示下一页
        prev_txt: '上一页',	 //上一页文字
        next_txt: '下一页',   　//下一页文字
        show_start: false,      //显示首页
        show_end: false,        //显示尾页
        start_txt: '首页',      //首页显示文字
        end_txt: '尾页',        //末页显示文字
        show_left_info: true    //是否显示左侧总数信息
    };

})(jQuery);