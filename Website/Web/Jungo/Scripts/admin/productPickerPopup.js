/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {

    DR.SearchProduct = Backbone.Model.extend({

        parse: function(response) {
            response['timestamp'] = new Date().getTime();   // need to make the model different or else Backbone won't see that it changed and view will not update. Solves issue of hitting search for same keyword string
            return response;
        },

        sync: function (method, model, options) {
            var o = options || {};

            if (method == "read") {
                if (o.data) {
                    this.url = this.baseUrl + o.data.keywords;
                    if (o.data.page) this.url = this.url + '/' + o.data.page;
                    if (o.data.pageSize) this.url = this.url + '/' + o.data.pageSize;
                    o.data = {};
                }
            }

            Backbone.Model.prototype.sync.apply(this, [method, model, options]);
        }

    });
    
    //---  Product Picker  ---
    DR.ProductPickerView = Backbone.View.extend({

        events: {
            'click #btnSearchProducts': 'productSearchClicked',
            'keypress #SearchCriteria': 'productSearchKeyDown',
            'click #btnReset': 'resetClicked',
            'click .pagingControl .btnPrevious': 'previousClicked',
            'click .pagingControl .btnNext': 'nextClicked',
            'click #btnResetSelected': 'resetSelectedClicked',
            'click #btnDone': 'doneClicked'
        },

        initialize: function() {
            this.searchCriteriaTextBox = this.$('#SearchCriteria');
            this.searchCriteriaTextBox.focus();
            this.controlId = this.$el.data('control-id');
            this.mode = this.$el.data('mode');
            this.controller = this.$el.data('service-controller');
            this.configureModel();
            this.model.on('change', this.searchProductComplete, this);

            this.selectedItems = new Backbone.Collection();
            this.populateSelectedItems();

            this.pagingModel = new DR.ProductPagingModel({});
            this.pagingView = new productResultPagingView({ model: this.pagingModel });
            $(this.pagingView.$el).insertAfter(this.$('#searchResults'));
        },

        configureModel: function() {
            var baseUrl = this.$el.data('root-url');
            if (baseUrl == '/') {
                this.baseUrl = '';
            }
            this.createNewModel();
            this.model.baseUrl = baseUrl + this.controller + '/SearchProducts/';
        },

        createNewModel: function () {
            if (typeof this.model === 'undefined') {
                this.model = new DR.SearchProduct();
            }
            this.model.set({
                keywords: '',
                page: 1,
                pageSize: 10,
                totalPages: 0
            });
        },

        populateSelectedItems: function () {
            var itemsSelected = new Backbone.Collection(this.$el.data('selected-items'));
            itemsSelected.forEach(function (item) {
                this.createNewSelectedProduct(item.toJSON());
            }, this);
        },

        resetClicked: function () {
            this.configureModel();
            this.pagingModel.set(this.pagingModel.defaults);
            this.clearResults(false);
            this.searchingMsg(false);
            this.searchCriteriaTextBox.val('');
            this.searchCriteriaTextBox.focus();
        },

        clearResults: function (clearSelected) {
            $('#searchResults ul').empty();

            if (clearSelected != null && clearSelected == true) {
                $('#selectedResults ul').empty();
            }
        },

        searchingMsg: function (showMessage) {
            showMessage ? this.pagingModel.set({ isVisible: true, isSearching: true }) : this.pagingModel.set({ isVisible: false, isSearching: false });
        },

        searchFooterMsg: function (showMessage) {
            var pg = this.model.get('page');
            var totPages = this.model.get('TotalResultPages');
            this.pagingModel.set({
                isVisible: showMessage,
                hasResults: (totPages > 0),
                pageNum: pg,
                pageOf: totPages,
                hidePrev: (pg <= 1),
                hideNext: (pg >= totPages)
            });
        },

        previousClicked: function () {
            var page = this.model.get('Page') - 1;
            page = (page < 1) ? 1 : page;

            this.doSearch(page);
        },

        nextClicked: function () {
            var page = this.model.get('Page') + 1;
            var totalPages = this.model.get('TotalPages');
            page = (page > totalPages) ? totalPages : page;

            this.doSearch(page);
        },

        productSearchKeyDown: function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                this.doSearch(1);
                return false;
            }
        },

        productSearchClicked: function () {
            this.doSearch(1);
        },

        searchEntryIsValid: function () {
            var textboxVal = this.searchCriteriaTextBox.val();
            var test = /^[\w\s]+$/.test(textboxVal);
            return test;
        },

        //---  Do fetch to trigger the get request back into the Controller  ---        
        doSearch: function (pageToFetch) {
            if (!this.searchEntryIsValid()) {
                return false;
            }

            this.clearResults();
            this.searchingMsg(true);

            this.model.fetch({
                data: {
                    page: pageToFetch,
                    keywords: this.searchCriteriaTextBox.val()
                },
                error: $.proxy(this.searchProductError, this)
            });
        },

        //---  Search Completed  ---
        searchProductComplete: function () {
            this.searchingMsg(false);
            this.searchFooterMsg(true);

            // Handle JSON returned from "ProductPickerController.SearchProducts"                
            _.each(this.model.get('Product'), function (product) {
                var itemView = new productResultItemView({
                    model: product
                });

                //--- Handle individual item click
                itemView.on('itemClicked', this.productSelected, this);

                this.$('#searchResults ul').append(itemView.render().el);
            }, this);
        },

        searchProductError: function (jqxhr) {
            this.searchingMsg(false);
        },

        selectedItemsReset: function () {
            this.selectedItems.forEach(function (item) {
            }, this);
        },

        productSelected: function (item) {
            if (this.mode == 'Single') {
                window.opener.DR.ProductControlManager.ProductSelected({
                    ControlID: this.controlId,
                    Product: {
                        ID: item.model.Id,
                        ImageUrl: item.model.ProductImage,
                        Name: item.model.DisplayName,
                        Description: item.model.ShortDescription,
                        Url: "?productId=" + item.model.Id
                    }
                });
                window.close(false);
            } else {
                this.createNewSelectedProduct(item.model);
            }
        },

        createNewSelectedProduct: function (model) {
            if (this.isDuplicateSelectedItem(model)) {
                return;
            }

            var itemView = new productSelectedItemView({
                model: model
            });

            this.listenTo(itemView, 'itemClicked', this.onSelectedItemClicked);
            this.$('#selectedResults ul').append(itemView.render().el);
            this.selectedItems.add(model);
        },

        onSelectedItemClicked: function (item) {
            // Remove from backbone collection
            for (var i = 0; i < this.selectedItems.length; i++) {
                var model = this.selectedItems.models[i];
                if (model.get('Id') == item.model.Id) {
                    model.destroy();
                    break;
                }
            }

            // Remove the view
            item.remove();
        },

        resetSelectedClicked: function () {
            for (var i = this.selectedItems.length - 1; i >= 0; i--) {
                this.selectedItems.at(i).destroy();
            }

            $('#selectedResults ul').empty();
        },

        doneClicked: function () {
            window.opener.DR.ProductControlManager.ProductSelected({
                ControlID: this.controlId,
                Product: this.selectedItems.toJSON()
            });
            window.close(false);
        },

        isDuplicateSelectedItem: function (item) {
            for (var i = 0; i < this.selectedItems.length; i++) {
                var model = this.selectedItems.models[i];
                if (model.get('Id') == item.Id) {
                    return true;
                }
            }
            return false;
        }

    });

    var productResultItemView = Backbone.View.extend({

        tagName: 'li',

        className: 'node ng-scope',

        events: {
            'click': 'viewClicked'
        },

        template: _.template($('#ac-item-template').html() || ''),

        render: function () {
            this.$el.html(this.template(this.model));
            return this;
        },

        viewClicked: function () {
            this.trigger('itemClicked', this);
        }

    });

    var productSelectedItemView = Backbone.View.extend({

        tagName: 'li',

        className: 'node ng-scope',

        events: {
            'click': 'viewClicked'
        },

        template: _.template($('#prod-selected-item-template').html() || ''),

        render: function () {
            this.$el.html(this.template(this.model));
            return this;
        },

        viewClicked: function () {
            this.trigger('itemClicked', this);
        }

    });

    DR.ProductPagingModel = Backbone.Model.extend({

        defaults: {
            isVisible: false,
            isSearching: false,
            hasResults: false,
            hidePrev: false,
            hideNext: false,
            pageNum: 0,
            pageOf: 0
        }

    });

    var productResultPagingView = Backbone.View.extend({
        
        className: 'pagingControl',

        template: _.template($('#paging-template').html() || ''),

        initialize: function () {
            this.model.on('change', this.render, this);
        },

        render: function () {
            this.$el.html(this.template(this.model.toJSON()));
            this.setElement(this.$el);
            return this;
        }

    });

    $(function () {
        if ($('#product-picker').length > 0) {
            DR.ProductPicker = new DR.ProductPickerView({
                el: $('#product-picker')
            });
        }
    });

})(DR || (DR = {}));