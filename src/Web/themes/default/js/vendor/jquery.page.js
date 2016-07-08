(function ($) {

    $.fn.paging = function (opts) {
        opts = $.extend(true, {}, $.fn.paging.defaults, opts);

        if (opts.total_count == 0) return false;
        if (opts.page_size == 0) return false;

        var page_count = Math.ceil(opts.total_count / opts.page_size),
            html = $('<div class="x-paging-"></div>'),
            page_html = '';

        if (opts.show_left_info) {
            var info = $('<div></div>');

            html.append(info);
        }
        
    };

    $.fn.paging.defaults = {
        total_count: 0,         //总数
        page_size: 10,          //分页大小
        show_prev: true,        //显示上一页
        show_next: true,        //显示下一页
        prev_txt: '<<',	        //上一页文字
        next_txt: '>>',         //下一页文字
        show_start: false,      //显示首页
        show_end: false,        //显示尾页
        show_left_info: true    //是否显示左侧总数信息
    };

})(jQuery);