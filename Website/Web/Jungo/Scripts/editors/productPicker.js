/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

// ReSharper disable InconsistentNaming
    var ProductPickerView = Backbone.View.extend({
// ReSharper restore InconsistentNaming
        
        events: {
            'click .product-selector-button': 'selectClicked'
        },

        initialize: function () {
            this.$pickerButton = $('.product-selector-button');

            this.map = this.$pickerButton.data('property-map');
            this.parentCategoryId = this.$pickerButton.data('parent-category-id');
            this.isMultiSelect = this.$pickerButton.data('is-multi-select');
            
            this.appPath = this.$pickerButton.data('app-path');
            if (this.appPath == '/')
                this.appPath = '';
        },

        selectClicked: function () {
            var mappedProperty = this.map['Id'];
            if (_.isArray(mappedProperty)) {
                mappedProperty = mappedProperty[0];
            }
            
            var ids = this.getProperty(mappedProperty);
            if (ids != null) {
                ids = ids.split(' ').join();
            }
            
            var url = this.appPath + '/ProductPicker/' + this.model.get('controlId') + (this.isMultiSelect ? '/Multi' : '/Single') + (ids != null ? '/' + ids : '');

            if (typeof this.productPickerPopup !== 'undefined' && !this.productPickerPopup.closed && this.productPickerPopup.focus) {
                this.productPickerPopup.focus();
            } else {
                this.productPickerPopup = window.open(url, null, 'height=600,width=400,resizable=yes,status=yes,scrollbars=yes');
            }

            return false;
        },

        updateControls: function (data) {
            var itemToMap = data;

            if(_.isArray(data)) {
                itemToMap = data[0];
                
                var i;
                for(i = 1; i < data.length; i ++) {
                    data[0].Id += " " + data[i].Id;                     
                }                    
            }

            this.mapDataToControls(itemToMap);
        },

        mapDataToControls: function(data) {
            if (data) {
                _.each(_.keys(data), function(key) {
                    var propValue = data[key];
                    var mappedProperty = this.map[key];
                    if (_.isArray(mappedProperty)) {
                        _.each(mappedProperty, function(mp) {
                            this.setProperty(mp, propValue);
                        }, this);
                    } else {
                        this.setProperty(mappedProperty, propValue);
                    }
                }, this);
            } else {
                var propValue = '';
                _.each(_.keys(this.map), function(key) {
                    var mappedProperty = this.map[key];
                    if (_.isArray(mappedProperty)) {
                        _.each(mappedProperty, function(mp) {
                            this.setProperty(mp, propValue);
                        }, this);
                    } else {
                        this.setProperty(mappedProperty, propValue);
                    }
                }, this);
            }
        },

        setProperty: function (propertyName, value) {
            this.$('input[type="text"][id*="_' + propertyName + '"],textarea[id*="_' + propertyName + '"]').val(value);
        },
        
        getProperty: function(propertyName) {
            return this.$('input[type="text"][id*="_' + propertyName + '"],textarea[id*="_' + propertyName + '"]').val();
        }

    });

    var productControlManager = function ($pickers) {
        this.pickers = [];

        _.each($pickers, function ($picker, index) {
            var view = new ProductPickerView({
                el: $picker,
                model: new Backbone.Model({
                    controlId: index
                })
            });

            this.pickers.push(view);
        }, this);
    };

    productControlManager.prototype.ProductSelected = function (p) {
        this.pickers[p.ControlID].updateControls(p.Product);
    };

    $(function () {
        var $pickers = $('.product-selector-button').parent().parent();
        if ($pickers.length > 0) {
            DR.ProductControlManager = new productControlManager($pickers);
        }
    });

})(DR || (DR = {}));
