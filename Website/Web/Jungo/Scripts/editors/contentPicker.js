/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

    var ContentPickerView = Backbone.View.extend({

        events: {
            'click .content-selector-button': 'selectContentClicked'
        },

        initialize: function () {
            this.$pickerButton = $('.content-selector-button');
            
            this.map = this.$pickerButton.data('property-map');
            this.startPageId = this.$pickerButton.data('start-page-id');

            this.appPath = this.$pickerButton.data('app-path');
            if (this.appPath == '/')
                this.appPath = '';
        },
        
        selectContentClicked: function() {
            var url = this.appPath + '/ContentPicker/' + this.model.get('controlId') + '/' + this.startPageId;

            if (typeof this.contentPickerPopup !== 'undefined' && !this.contentPickerPopup.closed && this.contentPickerPopup.focus) {
                this.contentPickerPopup.focus();
            } else {
                this.contentPickerPopup = window.open(url, null, 'height=600,width=400,resizable=yes,status=yes,scrollbars=yes');
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
        }

    });

    var contentControlManager = function(pickers) {
        this.pickers = [];
        
        _.each(pickers, function(pickerItem, index) {
            var view = new ContentPickerView({
                el: pickerItem,
                model: new Backbone.Model({
                    controlId: index
                })
            });

            this.pickers.push(view);
        }, this);
    };

    contentControlManager.prototype.ContentSelected = function(p) {
        this.pickers[p.ControlID].updateControls(p.Content);
    };


    $(function () {
        var pickers = $('.content-selector-button').parent().parent();
        if (pickers.length > 0) {
            DR.ContentControlManager = new contentControlManager(pickers);
        }
    });

})(DR || (DR = {}));
