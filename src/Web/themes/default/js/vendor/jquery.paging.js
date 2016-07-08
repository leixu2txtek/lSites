(function ($) {

    $.fn.paging = function (opts) {
        opts = $.extend(true, {}, $.fn.paging.defaults, opts);

        if (opts.total_count == 0) return false;
        if (opts.page_size == 0) return false;

        //渲染HTML        
        var render_html = function () {

            var page_count = Math.ceil(opts.total_count / opts.page_size),
                container = '<div class="x-paging"></div>',
                left = '<div class="x-paging-pagination-left">共 <span style="height:20px;">{{total_count}}</span> 条，每页显示<select name="pageSize" style="width: 50px; margin: 0px 5px;"><option value="10">10</option><option value="20">20</option><option value="50">50</option><option value="100">100</option></select>条</div>',
                right = '<ul class="x-paging-right"></ul>';

            //render left
            if (opts.show_left_info) {

                left = left.replace('{{total_count}}', opts.total_count);
                left = $(left);

                $('select', left).val(opts.page_size);
            }

            //render right
            right = $(right);

            //start & end
            opts.show_start && right.append('<li><a href="javascript:void(0);" data-page="1">' + opts.start_txt + '</a></li>');
            opts.show_end && right.append('<li><a href="javascript:void(0);" data-page="' + page_count + '">' + opts.end_txt + '</a></li>');

            
        };

        return $(this).each(function (i, v) {

            var page_count = Math.ceil(opts.total_count / opts.page_size),
                html = $('<div class="x-paging"></div>'),
                page_html = $('<ul class="x-paging-right"></ul>');

            //render left info            
            if (opts.show_left_info) {
                var left = '<div class="x-paging-pagination-left">共 <span style="height:20px;">{{total_count}}</span> 条，每页显示<select name="pageSize" style="width: 50px; margin: 0px 5px;"><option value="10">10</option><option value="20">20</option><option value="50">50</option><option value="100">100</option></select>条</div>';

                left = left.replace('{{total_count}}', opts.total_count);
                left = $(left);

                $('select', left).val(opts.page_size);

                html.append(left);
            }

            //render right info
            opts.show_start && page_html.append('<li><a href="javascript:void(0);" data-page="1">' + opts.start_txt + '</a></li>');
            opts.show_end && page_html.append('<li><a href="javascript:void(0);" data-page="' + page_count + '">' + opts.end_txt + '</a></li>');



            html.append(page_html);
            $(this).replaceWith(html);
        });
    };

    $.fn.paging.defaults = {
        total_count: 0,         //总数
        page_size: 10,          //分页大小
        show_prev: true,        //显示上一页
        show_next: true,        //显示下一页
        prev_txt: '<<',	        //上一页文字
        next_txt: '>>',         //下一页文字
        show_start: true,      //显示首页
        show_end: true,        //显示尾页
        start_txt: '首页',      //首页显示文字
        end_txt: '尾页',        //末页显示文字
        show_left_info: true    //是否显示左侧总数信息
    };

})(jQuery);