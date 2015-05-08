/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />
/// <reference path="../site.js" />

var DR;

(function (DR) {

    var productDetailsView = Backbone.View.extend({

        events: {
            'click #buy-button': 'processAddToCart'
        },

        initialize: function () {
            this.$buyButton = this.$('#buy-button');
            this.actionsDisabled = false;
            this.shoppingCartLink = this.$el.data('shopping-cart-link');
            this.isSiteCart = this.$el.data('is-site-cart');
            this.productId = this.$el.data('product-id');
            this.imageUrlPrefix = this.$el.data('image-url-prefix');
            this.createVariationSelector();
        },

        variationOptionChanged: function(product) {
            if (!this.actionsDisabled) {
                this.updateProductDetails(product);
            }
        },

        createVariationSelector: function () {
            var $vc = this.$('.variation-control');
            if ($vc.length > 0) {
                this.variationsModel = new Backbone.Model(this.$el.data('variations'));
                var variations = this.variationsModel.get('Variations');
                this.variationControl = new variationControl({
                    el: $vc,
                    model: variations,
                    selectedProductId: this.productId
                });
                this.updateProductDetails(this.variationControl.selectedProduct());
                this.variationControl.on('change', this.variationOptionChanged, this);
            }
        },

        processAddToCart: function (elm) {
            this.actionsDisabled = true;
            this.addingToCart();

            if (this.isSiteCart) {
                var productId = typeof this.variationControl === 'undefined' ? this.productId : this.variationControl.selectedProduct().Id;
                var link = this.shoppingCartLink + '/buy?productIds=' + productId + '&quantity=1';
                window.location.href = link;
            }
        },

        updateProductDetails: function (product) {
            this.$('#product-images-wrapper img').attr('src', this.getProductImageFromAttribute(product, 'productImage3', product.ThumbnailImage));
            this.$('#product-current-price').text(product.Pricing.FormattedListPriceWithQuantity);
            this.$('#form-product-id').attr('value', product.Id);
        },

        addingToCart: function() {
            this.$el.addClass('adding-to-cart');
            this.$buyButton.bootstrapBtn('addcart');
        },

        // Because Product.ThumbnailImage and Product.ProductImage were designed for sites with simple image requirements (thumbnail and detail),
        // we use this function to pull out images from the product attributes when multiple image sizes are needed. For the demo store, we have at least
        // three sizes to choose from.
        getProductImageFromAttribute: function (product, attributeName, fallbackUrl) {
            var ret;
            var attr = _.find(product.CustomAttributes.Attribute, function (attribute) { return attribute.Name === attributeName; });
            if (typeof (attr) === 'undefined') {
                ret = fallbackUrl;
            } else {
                ret = attr.Value;
            }

            if (!(ret.substr(0, 4) === 'http')) {
                ret = this.imageUrlPrefix + ret;
            }

            return ret;
        }

    });

    var variationControl = Backbone.View.extend({
        events: {
        },

        initialize: function () {
            var optionControlId = 'option-control-' + this.model.id;
            var $rootOptionControl = $('#' + optionControlId);
            this.rootOptionControl = new optionControl({
                el: $rootOptionControl,
                model: this.model
            });
            
            this.rootOptionControl.show();
            this.selectProduct(this.options.selectedProductId);
            this.rootOptionControl.on('change', this.optionChanged, this);
        },

        optionChanged: function() {
            this.trigger('change', this.rootOptionControl.selectedProduct());
        },

        selectedProduct: function() {
            return this.rootOptionControl.selectedProduct();
        },

        selectProduct: function(pid) {
            this.rootOptionControl.trySelectProduct(pid);
        }
    });

    var optionControl = Backbone.View.extend({
        //events: {
        //    'change input': function () { alert('changed'); }
        //},

        initialize: function () {
            this.suppressEvents = false;
            this.selectedOption = null;
            this.previousId = (typeof this.options.previousId === 'undefined') ? '' : this.options.previousId;
            this.variationId = (this.previousId === '' ? '' : this.previousId + '-') + this.model.id;
            this.optionControlId = 'option-control-' + this.variationId;
            this.optionControls = {};
            this.optionButtons = [];
            _.each(this.model.options, function (option) {
                option.safeOptionValue = option.value.replace('.', '_');
                option.safeOptionValue = option.safeOptionValue.replace(' ', '_');
                var itemFullName = this.variationId + '-' + option.safeOptionValue;
                var optionButton = new variationButtonControl({
                    el: this.$('#btn' + itemFullName),
                    model: option
                });
                optionButton.on('change', this.onOptionButtonClicked, this);
                this.optionButtons.push(optionButton);

                if (option.variations != null) {
                    var childVariationId = this.optionControlId + '-' + option.safeOptionValue + '-' + option.variations.id;
                    var childOptionControl = new optionControl({
                        el: $('#' + childVariationId),
                        model: option.variations,
                        previousId: this.variationId + '-' + option.value
                    });
                    childOptionControl.on('change', function(p) { this.trigger('change', p); }, this);
                    this.optionControls[option.safeOptionValue] = childOptionControl;
                }
            }, this);
        },

        onOptionButtonClicked: function (optionButton) {
            this.selectedOption = optionButton;
            this.hideChildren();
            this.showSelected();

            if (!this.suppressEvents)
                this.trigger('change');
        },

        show: function () {
            if (this.selectedOption === null) {
                this.suppressEvents = true;
                this.optionButtons[0].select();
                this.suppressEvents = false;
            }

            this.$el.removeClass('hidden');
            this.$el.show();
            this.showSelected();
        },

        showSelected: function() {
            var selected = this.optionControls[this.selectedOption.value];
            if (selected != null)
                selected.show();
        },

        hide: function() {
            this.$el.hide();
            this.$el.addClass('hidden');
            this.hideChildren();
        },

        hideChildren: function() {
            _.each(this.model.options, function (option) {
                var optionControl = this.optionControls[option.safeOptionValue];
                if (optionControl != null) optionControl.hide();
            }, this);
        },

        selectedProduct: function () {
            if (this.selectedOption.product != null) return this.selectedOption.product;

            var selected = this.optionControls[this.selectedOption.model.safeOptionValue];
            if (typeof selected !== 'undefined') return selected.selectedProduct();

            return null;
        },

        trySelectProduct: function (pid) {
            if (_.isEmpty(this.optionControls)) {
                var buttonToSelect = _.find(this.optionButtons, function(optionButton) { return optionButton.product.Id === pid; });

                if (typeof buttonToSelect === 'undefined') {
                    return false;
                }

                this.suppressEvents = true;
                buttonToSelect.select();
                this.suppressEvents = false;

                return true;
            }

            buttonToSelect = _.find(this.optionButtons, function (optionButton) {
                return this.optionControls[optionButton.value].trySelectProduct(pid);
            }, this);
            
            if (typeof buttonToSelect !== 'undefined') {
                this.suppressEvents = true;
                buttonToSelect.select();
                this.suppressEvents = false;
                return true;
            }

            return false;
        }
    });

    var variationButtonControl = Backbone.View.extend({
        events: {
            'change input' : 'onSelected'
        },

        initialize: function () {
            this.initialized = false;
            this.value = this.model.value;
            this.product = this.model.product;
        },

        onSelected: function () {
            this.trigger('change', this);
        },

        select: function() {
            this.$el.bootstrapBtn('toggle');
        }
    });

    DR.ProductDetailsView = productDetailsView;

})(DR || (DR = {}));