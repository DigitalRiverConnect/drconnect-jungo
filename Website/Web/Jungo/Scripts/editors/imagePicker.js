/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

    var ImagePickerView = Backbone.View.extend({

        events: {
            'click .image-selector-button': 'selectImageClicked'
        },

        initialize: function () {
            this.$pickerButton = this.$('.image-selector-button');
            this.appPath = this.$pickerButton.data('app-path');
            if (this.appPath == '/')
                this.appPath = '';
            this.pickerTextBoxId = this.$pickerButton.data('image-control-id');
        },
        
        selectImageClicked: function() {
            var url = this.appPath + '/ImagePicker/' + this.model.get('controlId');

            if (typeof this.imagePickerPopup !== 'undefined' && !this.imagePickerPopup.closed && this.imagePickerPopup.focus) {
                this.imagePickerPopup.focus();
            } else {
                this.imagePickerPopup = window.open(url, null, 'height=600,width=700,resizable=yes,status=yes,scrollbars=yes');
            }
            
            return false;
        },
       
        setImagePath: function(value) {
            this.$('input[type="text"][id*="_' + this.pickerTextBoxId + '"]').val(value);
        }

    });

    var imageControlManager = function(pickerElements) {
        this.pickers = [];
        
        _.each(pickerElements, function(picker, index) {
            var view = new ImagePickerView({
                el: picker,
                model: new Backbone.Model({
                    controlId: index
                })
            });

            this.pickers.push(view);
        }, this);
    };

    imageControlManager.prototype.ImageSelected = function(p) {
        this.pickers[p.ControlID].setImagePath(p.FilePath);
    };

    $(function () {
        var pickers = $('.image-selector-button').parent();
        if (pickers.length > 0) {
            DR.ImageControlManager = new imageControlManager(pickers);
        }
    });

})(DR || (DR = {}));
