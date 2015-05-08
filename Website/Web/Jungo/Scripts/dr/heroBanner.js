/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />
/// <reference path="../site.js" />

var DR;

(function(DR) {

    var heroBannerView = Backbone.View.extend({
        
        initialize: function () {
            if (this.$('ul.slides li').length == 1) {
                this.$('.tab-container, .slider-track').remove();
            }
            this.$('ul.slides li:first-child').addClass('active');
        }

    });

    DR.HeroBannerView = heroBannerView;

    var bigHeroBladeBannerView = Backbone.View.extend({

        initialize: function () {
            var that = this;
            
            var bladeCount = this.$el.find('.hero.big-container > li').length;
            if(bladeCount > 1) {
                this.$el.find('.slider-tabs .grid-row').addClass('column-' + bladeCount);
            }
            var that = this;
            this.$el.find('.hero.big-container > li').each(function() {
                var bladeCaption = $(this).find('a.link').data('blade-caption');
                that.$el.find('.slider-tabs .grid-row').append('<div class="grid-unit"><a href="javascript:void(0)">' + bladeCaption + '</a></div>');
            });
            this.$el.find('.slider-tabs .grid-unit:first-child a').addClass('active');
            var sliderWidth = this.$el.find('.slider-tabs .grid-unit').css('width');
            this.$el.find('.blue-slider').css({ width: sliderWidth });
            
            var i = this.$el.find('.hero'),
                n, t;
            i.length !== 0 && (n = new SliderControl({
                autoRotate: !0,
                animate: !0,
                container: i,
                duration: 7
            }), Store.heroSlider = n, t = n.slides.find("a"), t.focus(function () {
                n.stop()
            }), t.blur(function () {
                n.start()
            }), n.start());
        }
    });

    DR.BigHeroBladeBannerView = bigHeroBladeBannerView;

})(DR || (DR = {}));