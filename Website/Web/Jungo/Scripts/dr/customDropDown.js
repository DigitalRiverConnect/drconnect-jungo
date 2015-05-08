/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

    var customDropdowns = Backbone.View.extend({

        _selectionListVisible: false,
        
        _isDisabled: false,
        
        events: {
            'click .selector': 'toggleSelectionList',
            'click .options li': 'selectionChanged'
        },
        
        initialize: function () {
            var elmId = this.$el.attr('id');
            var that = this;
            $(document).mouseup(function (e) {
                if (!$('#' + elmId + ' .options').is(e.target) && !$('#' + elmId + ' a').is(e.target) && that.optionsVisible()) {
                    $('#' + elmId + ' .options').css({ visibility: 'hidden' });
                    that.setOptionsVisibility(false);
                }
            });
        },

        toggleSelectionList: function () {
            if (!this._isDisabled) {
                if (this.optionsVisible()) {
                    this.hideSelectionList();
                } else {
                    this.showSelectionList();
                }
            }
        },
        
        hideSelectionList: function () {
            this.$el.find('.options').css('visibility', 'hidden');
            this.setOptionsVisibility(false);
        },
        
        showSelectionList: function () {
            this.$el.find('.options').css('visibility', 'visible');
            this.setOptionsVisibility(true);
        },

        selectionChanged: function (elm) {
            this.hideSelectionList();
            this.$el.find('.chosen .content').html($(elm.currentTarget).html());
            if (typeof this.options.callback === 'function') {
                this.options.callback(elm);
            }
        },

        setOptionsVisibility: function (value) {
            this._selectionListVisible = value;
        },

        optionsVisible: function () {
            return this._selectionListVisible;
        },
        
        doEnable: function () {
            this.$el.find('.selector').removeClass('inactive');
            this._isDisabled = false;
        },
        
        doDisable: function () {
            this.$el.find('.selector').addClass('inactive');
            this.$el.find('.options').css('visibility', 'hidden');
            this._isDisabled = true;
        }

    });

    DR.CustomDropdownView = customDropdowns;
    
})(DR || (DR = {}));