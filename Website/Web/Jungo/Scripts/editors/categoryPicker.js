/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

    var CategoryPickerView = Backbone.View.extend({

        events: {
            'click .category-selector-button': 'selectCategoryClicked'
        },

        initialize: function () {
            this.$pickerButton = $('.category-selector-button');
            
            this.map = this.$pickerButton.data('property-map');
            this.parentCategoryId = this.$pickerButton.data('parent-category-id');

            this.appPath = this.$pickerButton.data('app-path');
            if (this.appPath == '/')
                this.appPath = '';
        },
        
        selectCategoryClicked: function () {
            var mappedProperty = this.map['ID'];
            if (_.isArray(mappedProperty)) {
                mappedProperty = mappedProperty[0];
            }

            var ids = this.getProperty(mappedProperty);
            if (ids != null) {
                ids = ids.split(' ').join();
            }
            
            var url = this.appPath + '/CategoryPicker/' + this.model.get('controlId');
            if(this.parentCategoryId)
                url = url + '/' + this.parentCategoryId;
            
            if (typeof this.categoryPickerPopup !== 'undefined' && !this.categoryPickerPopup.closed && this.categoryPickerPopup.focus) {
                this.categoryPickerPopup.focus();
            } else {
                this.categoryPickerPopup = window.open(url, null, 'height=600,width=400,resizable=yes,status=yes,scrollbars=yes');
            }

            return false;
        },
        
        updateControls: function(data) {
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
        },
        
        setProperty: function(propertyName, value) {
            this.$('input[type="text"][id*="_' + propertyName + '"],textarea[id*="_' + propertyName + '"]').val(value);
        },

        getProperty: function(propertyName) {
            return this.$('input[type="text"][id*="_' + propertyName + '"],textarea[id*="_' + propertyName + '"]').val();
        }

    });

    var categoryControlManager = function(pickerElements) {
        this.pickers = [];
        
        _.each(pickerElements, function(picker, index) {
            var view = new CategoryPickerView({
                el: picker,
                model: new Backbone.Model({
                    controlId: index
                })
            });

            this.pickers.push(view);
        }, this);
    };

    categoryControlManager.prototype.CategorySelected = function(p) {
        this.pickers[p.ControlID].updateControls(p.Category);
    };


    $(function () {
        var pickers = $('.category-selector-button').parent().parent();
        if (pickers.length > 0) {
            DR.CategoryControlManager = new categoryControlManager(pickers);
        }
    });

})(DR || (DR = {}));
