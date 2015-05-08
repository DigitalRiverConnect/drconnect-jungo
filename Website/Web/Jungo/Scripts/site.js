/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />
/// <reference path="~/Scripts/dr/autocomplete.js" />

// http://paulirish.com/2011/requestanimationframe-for-smart-animating/
// http://my.opera.com/emoller/blog/2011/12/20/requestanimationframe-for-smart-er-animating

// requestAnimationFrame polyfill by Erik Möller. fixes from Paul Irish and Tino Zijdel

// MIT license

(function () {
    var lastTime = 0;
    var vendors = ['ms', 'moz', 'webkit', 'o'];
    for (var x = 0; x < vendors.length && !window.requestAnimationFrame; ++x) {
        window.requestAnimationFrame = window[vendors[x] + 'RequestAnimationFrame'];
        window.cancelAnimationFrame = window[vendors[x] + 'CancelAnimationFrame']
                                   || window[vendors[x] + 'CancelRequestAnimationFrame'];
    }


    if (!window.requestAnimationFrame)
        window.requestAnimationFrame = function (callback, element) {
            var currTime = new Date().getTime();
            var timeToCall = Math.max(0, 16 - (currTime - lastTime));
            var id = window.setTimeout(function () { callback(currTime + timeToCall); },
              timeToCall);
            lastTime = currTime + timeToCall;
            return id;
        };

    if (!window.cancelAnimationFrame)
        window.cancelAnimationFrame = function (id) {
            clearTimeout(id);
        };
}());

var DR;

(function (DR) {

    DR.getURLParameter = function (name) {
        return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [, ""])[1].replace(/\+/g, '%20')) || '';
    };

    DR.initializeRouter = function(routeData, root) {
        var router = Backbone.Router.extend(routeData);

        DR.Router = new router();

        var r = root || location.pathname;

        Backbone.history.start({ pushState: false, root: r });
    }



    $(function () {
        var $searchContainer = $('#search-container');
        if ($searchContainer.length > 0) {
            DR.SearchBoxPart = new DR.SearchBoxView({
                el: $searchContainer
            });
        }

        var $productDetailsContainer = $('#product-details');
        if ($productDetailsContainer.length > 0) {
            DR.ProductDetailsPart = new DR.ProductDetailsView({
                el: $productDetailsContainer
            });
        }

        var $heroBannerContainer = $('#heroBannerContainer');
        if ($heroBannerContainer.length > 0) {
            DR.HeroBannerPart = new DR.HeroBannerView({
                el: $heroBannerContainer
            });
        }

        var customBundleContainer = $('#product-picker');
        if (customBundleContainer.length > 0) {
            DR.BundleProductPickerPart = new DR.BundleProductPickerView({
                el: customBundleContainer
            });
        }
    });

})(DR || (DR = {}));

// prevent clickjack
(function () {
    var externallyFramed = false;
    try {
        externallyFramed = top.location.host != location.host;
    }
    catch (err) {
        externallyFramed = true;
    }
    if (externallyFramed) {
        top.location = location;
    }
})();
