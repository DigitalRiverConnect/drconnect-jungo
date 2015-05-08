/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />
/// <reference path="~/Scripts/dr/autocomplete.js" />
/// <reference path="~/Scripts/moment.js" />

var DR;

(function (DR) {

    var localizeTimeElements = function() {
        $('time').each(function() {
            var item = $(this);
            var rawTime = item.data('value');
            if (rawTime) {
                var format = item.data('format');
                var m = moment(rawTime, 'YYYY-MM-DD H:mm:ss Z');
                item.text(m.format(format));
            }
        });
    };

    // Call anywhere through DR.updateUTCTimeElements();
    DR.localizeTimeElements = localizeTimeElements;

    $(function() {
        var culture = $('html').attr('culture');
        if(culture && culture != 'en-US')
            moment.lang(culture);
        
        if ($('time').length > 0) {
            localizeTimeElements();
        }
    });
})(DR || (DR = {}));