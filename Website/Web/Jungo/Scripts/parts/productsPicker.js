(function ($) {
    "use strict";

    // sweber@digitalriver.com - responsive/resizing version
    $.productsPickerHandler = function (element) {
        var listVarsArr = []; // variables for each tab
        var currVars;  // variables for current tab
        var index = 0;
        var sliding = false;
        var pageSize;
        var itemWidth;

        // init variables for one tab - pagesize and leftpos will change on resize!
        var initVars = function (index, pageSize) {
            var count = $(element).find('.list ul').eq(index).find('li').length;

            return { "count": count, "leftPos": 0, "leftIndex": 0, "page": 1, "totPages": Math.ceil(count / pageSize) };
        };

        // move tab picker indicator and show respective ul
        var managePicker = function () {
            $(element).find('.picker span.pointer').css({ 'visibility': 'hidden' });
            $(element).find('.picker span.pointer').eq(index).css({ 'visibility': 'visible' });
            $(element).find('.list ul').css({ 'display': 'none' });
            $(element).find('.list ul').eq(index).css({ 'display': 'block' });
        };

        var enablePaging = function (pager) {
            $(pager).removeClass('disabled').addClass('enabled');
        };

        var disablePaging = function (pager) {
            $(pager).removeClass('enabled').addClass('disabled');
        };

        // update paging controls
        var managePaging = function () {
            // show/hide page control
            if (currVars.totPages === 1) {
                //console.log("hide paging");
                $(element).find('.pageControl').hide(); // css({ 'display': 'none' });
            } else {
                //console.log("show paging " + currVars.page + "/" + currVars.totPages);
                $(element).find('.pageControl').show(); // css({ 'display': 'block' }); // inline- keeps both together but floats left

                // en/disable left/right controls
                if (currVars.page < currVars.totPages) {
                    enablePaging($(element).find('.pageControl span.right'));
                } else {
                    disablePaging($(element).find('.pageControl span.right'));
                }
                if (currVars.page > 1 && currVars.page <= currVars.totPages) {
                    enablePaging($(element).find('.pageControl span.left'));
                } else {
                    disablePaging($(element).find('.pageControl span.left'));
                }
            }
        };

        // move list to show items starting with leftIndex
        var slideList = function (delay) {
            sliding = true;

            if (currVars.leftIndex < 0) {
                currVars.leftIndex = 0;
            }
            else if (currVars.leftIndex > currVars.count - pageSize) {
                currVars.leftIndex = currVars.count - pageSize;
            }
            currVars.leftPos = (-1 * currVars.leftIndex * (itemWidth + 10));

            var ul = $(element).find('.list ul').eq(index);
            $(ul).animate({ left: currVars.leftPos + 'px' }, delay, function () {
                managePaging();
                sliding = false;
            });
        };

        var resize = function (force) {
            // new width/page size
            var listWidth = $(element).find('.list').width();
            var newPageSize = (listWidth < 540) ? 2 : 4; // Math.floor(listWidth / targetItemWidth);
            itemWidth = Math.floor(listWidth / newPageSize) - 10; // margin 10px
            $(element).find('.list ul > li').css({ 'width': itemWidth + 'px' });

            //console.log("resize to " + listWidth + " -> " + itemWidth + "px * " + newPageSize);

            // pagesize change -> update vars
            if (force || pageSize !== newPageSize) {
                pageSize = newPageSize;

                if (typeof currVars === 'undefined') {
                    currVars = initVars(index, pageSize);
                } else {
                    currVars.totPages = Math.ceil(currVars.count / pageSize);
                }
                managePaging();
            }

            if (currVars.leftIndex !== 0) {
                slideList(0); // adjust left offset
            }
        };

        // init control
        managePicker();
        resize(true);

        // resize event
        // won't work $(element).resize(function () 

        // resize event
        $(window).resize(function () {
            resize(false);
        });

        // page right
        $(element).find('.pageControl span.right').click(function () {
            if (!sliding && $(this).hasClass('enabled')) {
                currVars.page += 1;
                currVars.leftIndex += pageSize;
                slideList(250);
            }
        });

        // page left
        $(element).find('.pageControl span.left').click(function () {
            if (!sliding && $(this).hasClass('enabled')) {
                currVars.page -= 1;
                currVars.leftIndex -= pageSize;
                slideList(250);
            }
        });

        // tab control selection
        $(element).find('.picker span.content').click(function () {
            listVarsArr[index] = currVars;
            index = $(this).closest('li').index();
            currVars = listVarsArr[index];
            //console.log("switch to " + index);

            managePicker();
            resize(true);
        });
    };
})(jQuery);