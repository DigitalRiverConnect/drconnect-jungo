/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

$.extend($.expr[':'], {
    'containsi': function (elem, i, match, array) {
        return (elem.textContent || elem.innerText || '').toLowerCase()
        .indexOf((match[3] || "").toLowerCase()) >= 0;
    }
});

var DR;

(function (DR) {

    var navTreeHandler = function (element) {
        this.element = element;
    };

    navTreeHandler.prototype.toggleSubTree = function (toggler) {
        var parent = toggler.parents('li').eq(0), newClass = '', oldClass = '', sublist = toggler.parents('li').eq(0).find('ul.targettable');
        if (parent.hasClass('expanded')) {
            newClass = 'collapsed';
            oldClass = 'expanded';
        } else {
            newClass = 'expanded';
            oldClass = 'collapsed';
        }
        parent.removeClass(oldClass);
        parent.addClass(newClass);
        sublist.removeClass(oldClass);
        sublist.addClass(newClass);
    };

    navTreeHandler.prototype.processSearch = function (searchFilter) {
        if (searchFilter) {
            this.element.addClass('searching');
            var elms = this.element.find('li.node a.link:containsi("' + searchFilter + '")').parents('div.item').addClass('foundit');
            this.checkForNoResults();
        } else {
            this.element.removeClass('searching');
            this.element.find('div.item').removeClass('foundit');
        }
    };

    navTreeHandler.prototype.checkForNoResults = function () {
        if (this.element.find('li div.item:visible').length == 0) {
            this.element.parent().addClass('no-results');
        } else {
            this.element.parent().removeClass('no-results');
        }
    };

    navTreeHandler.prototype.selectNode = function (selectedNode) {
        this.element.find('a').removeClass('selected');
        selectedNode.addClass('selected');
    };

    DR.PickerView = Backbone.View.extend({
        events: {
            'click #btnReset': 'resetClicked',
            'click #btnSearch': 'searchClicked',
            'click .node a.toggle': 'toggleTree',
            'click .node a.link': 'selectNode',
            'keydown #SearchCriteria': 'searchCriteriaKeyPressed'
        },

        initialize: function () {
            this.navTree = new navTreeHandler(this.$el.find('#page-tree > .targettable'));
            this.searchCriteria = this.$('#SearchCriteria');
        },

        searchClicked: function () {
            this.navTree.processSearch('');
            this.navTree.processSearch(this.searchCriteria.val());
            this.navTree.checkForNoResults();
        },

        resetClicked: function () {
            this.searchCriteria.val('');
            this.searchClicked();
            this.navTree.checkForNoResults();
        },

        toggleTree: function (elm) {
            this.navTree.toggleSubTree($(elm.currentTarget));
        },

        selectNode: function (elm) {
            this.navTree.selectNode($(elm.currentTarget));
            this.options.selectedANodeCallback.call(this, $(elm.currentTarget));
        },

        searchCriteriaKeyPressed: function (e) {
            if (e.keyCode == 13) {
                this.searchClicked();
                return false;
            }
        }
    });

    DR.SearchProduct = Backbone.Model.extend({
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

    DR.Product = Backbone.Model.extend();

    DR.ImagePreviewModel = Backbone.Model.extend({

        defaults: {
            date: '',
            href: '',
            name: '',
            source: '',
            img_width: 0,
            img_height: 0
        }

    });

    DR.ImagePreviewView = Backbone.View.extend({

        template: _.template($('#image-preview-template').html() || ''),

        className: 'image-preview-wrapper modal hide fade',

        events: {
            'click #imagePreviewClose': 'closeImagePreview',
            'click #btnUseImage': 'useThisImage'
        },

        initialize: function () {
            _.bindAll(this);
            var img = new Image();
            var that = this;
            img.onload = function () {
                that.model.set({ img_width: img.width });
                that.model.set({ img_height: img.height });
                that.render();
            };
            img.src = this.model.get('source');
        },

        render: function () {
            this.$el.html(this.template(this.model.toJSON()));
            this.$el = this.$el.children();
            this.setElement(this.$el);
            return this;
        },

        closeImagePreview: function () {
            this.$el.modal('hide');
        },

        reveal: function () {
            this.$el.modal({
                keyboard: false,
                backdrop: 'static'
            });
        },

        tearDown: function () {
            $('#' + this.className).remove();		// TODO - probably a better way to clean up the modals after they are hidden
        },

        useThisImage: function (elm) {
            var controlId = $('input[name="ControlId"]').val();
            var filepath = $(elm.currentTarget).data('image-file-path');
            // Pass back the image info from the selected node to the Parent Window & close
            window.opener.DR.ImageControlManager.ImageSelected({
                ControlID: controlId,
                FilePath: filepath
            });
            window.close(false);
        }

    });

    DR.PickerImage = Backbone.Model.extend({

        defaults: {
            FileDate: '',
            FileDateStr: '',
            FileName: '',
            FilePath: '',
            FileType: '',
            ImageUrl: '',
            Direction: 'down',
            img_width: 0,
            img_height: 0
        }

    });

    DR.PickerImageCollection = Backbone.Collection.extend({

        defaults: {
            model: DR.PickerImage
        },

        filterByFileType: function(type) {
            filtered = this.filter(function(image) {
                return image.get("FileType") === type;
            });
            return new DR.PickerImageCollection(filtered);
        },

        model: DR.PickerImage

    });

    DR.ImagePickerModel = Backbone.Model.extend({

        defaults: {
            ControlId: '',
            Images: new DR.PickerImageCollection(),
            SearchCriteria: '',
            SortOrder: 2,
            VirtualPath: '/upload',

        },

        initialize: function (options) {
            this.controller = options.controller;
            this.setupForFileSearch();
        },

        parse: function (response) {
            // Images collection comes back in array - need to put it into its own backbone collection so we can manage it easier
            if (typeof response.Images !== 'undefined') {
                this.set({ Images: new DR.PickerImageCollection(response.Images) });
            }
        },

        setupForKeywordSearch: function () {
            this.url = document.location.origin + '/' + this.controller + '/Search/';
        },

        setupForFileSearch: function () {
            this.url = document.location.origin + '/' + this.controller + '/GetFiles/';
        }

    });

    var FormControl = Backbone.View.extend({
        enable: function(v) {
            this.$el.prop('disabled', !v);
        }
    });

    var ImagePickerPopupView = Backbone.View.extend({

        events: {
            'click #btnSearch': 'getSearchImages',
            'keypress #SearchCriteria': 'getSearchImagesEnter',
            'click #btnReset': 'initialize',
            'click .details-box button': 'selectImage',
            'click #image-directories a': 'selectDirectory',
            'click #image-files a': 'previewImage',
            'click .image-preview a': 'selectPreviewImage',
            'click .sort-newest': 'sortByNewest',
            'click .sort-oldest': 'sortByOldest',
            'change #file' : 'fileChanged'
        },

        initialize: function () {
            this.clearUpPage();
            this.$('input[name="SearchCriteria"]').val('');
            this.controller = this.$el.data('service-controller');
            this.configureModel();
            this.getFileImages();
            this.file$el = this.$('#file');
            this.submitButton = new FormControl({
                el: '#upload-image'
            });
            this.enableUploadButton();
        },

        getOneDirectoryUp: function() {
            var path = this.model.get('VirtualPath');
            return path.substring(0, path.lastIndexOf('/'));
        },

        configureModel: function () {
            this.model = new DR.ImagePickerModel({ controller: this.controller });
        },

        sortByNewest: function() {
            this.$('.sort span').html('Newest first');
            this.model.set({ SortOrder: 2 });
        },

        sortByOldest: function () {
            this.$('.sort span').html('Oldest first');
            this.model.set({ SortOrder: 1 });
        },

        getFileImages: function (_filePath, direction) {
            this.clearUpPage();
            this.showLoadingIndicator();
            this.model.setupForFileSearch();
            if (typeof _filePath !== 'undefined' && direction == 'down') {
                this.model.set({ VirtualPath: _filePath });
            } else if (direction == 'up') {
                this.model.set({ VirtualPath: this.getOneDirectoryUp() });
            }
            this.$('#virtual-path').html(this.model.get('VirtualPath'));
            this.model.fetch({
                data: { path: this.model.get('VirtualPath'), sortOrder: this.model.get('SortOrder') },
                success: $.proxy(this.getImagesSuccess, this),
                error: $.proxy(this.getImagesError, this),
                type: 'POST'
            });
        },

        getSearchImages: function () {
            this.model.setupForKeywordSearch();
            this.clearUpPage();
            var searchCriteria = $('input[name="SearchCriteria"]').val();
            if (searchCriteria != '') {
                this.showLoadingIndicator();
                $('#virtual-path').html('Search keyword: ' + searchCriteria);
                this.model.fetch({
                    data: { virtualPath: this.model.get('VirtualPath'), searchCriteria: searchCriteria, sortOrder: this.model.get('SortOrder') },
                    success: $.proxy(this.getImagesSuccess, this),
                    error: $.proxy(this.getImagesError, this),
                    type: 'POST'
                });
            }
        },

        getSearchImagesEnter: function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                this.getSearchImages();
                return false;
            }
        },

        getImagesSuccess: function () {
            this.hideLoadingIndicator();
            // === Add in images
            var images = this.model.get('Images');
            images.filterByFileType('f').each(function (_model) {
                var view = new imageResultItemView({ model: _model });
                this.$('#image-files').append(view.$el);
            });

            // === Add in directories
            // parent directory not being returned at the moment - add it in here
            var upFilePath = this.getOneDirectoryUp();
            if (typeof upFilePath !== 'undefined' && upFilePath.length > 0) {
                var view = new directoryResultItemView({ model: new DR.PickerImage({ FilePath: upFilePath, FileType: 'p', FileName: 'Up a directory', Direction: 'up' }) });
                this.$('#image-directories').append(view.$el);
            }
            images.filterByFileType('d').each(function(_model) {
                var view = new directoryResultItemView({ model: _model });
                this.$('#image-directories').append(view.$el);
            });
        },

        getImagesError: function () {
            this.hideLoadingIndicator();
            // maybe load their last state here?
            this.initialize();
        },

        selectDirectory: function (elm) {
            this.getFileImages($(elm.currentTarget).data('file-path'), $(elm.currentTarget).data('path-direction'));
        },

        clearUpPage: function() {
            this.$('#image-directories').html('');
            this.$('#image-files').html('');
            this.$('.results .image-preview-wrapper').remove();
        },

        previewImage: function (elm) {
            var element = $(elm.currentTarget);
            var image = element.find('img');
            var ipm = new DR.ImagePreviewModel({ date: image.data('file-date'), source: image.attr('src'), name: image.data('file-name'), href: element.data('image-file-path') });
            var imagePreviewView = new DR.ImagePreviewView({ model: ipm });
            this.$('.results').append(imagePreviewView.$el);
            imagePreviewView.reveal();
        },

        showLoadingIndicator: function () {
            this.$('.results-wrapper').addClass('loading');
            var _height = ($(window).height() - 169);
            this.$('.results-wrapper').css({ height: _height + 'px' });
        },

        hideLoadingIndicator: function () {
            this.$('.results-wrapper').removeClass('loading');
            this.$('.results-wrapper').css({ height: 'auto' });
        },

        fileChanged: function() {
            this.enableUploadButton();
        },

        enableUploadButton: function() {
            var val = this.file$el.val();
            var isEnabled = typeof val !== 'undefined' && val !== null && val !== '';
            this.submitButton.enable(isEnabled);
        }

    });

    var directoryResultItemView = Backbone.View.extend({

        template: _.template($('#directory-template').html() || ''),

        initialize: function () {
            this.render();
        },

        render: function () {
            this.$el.html(this.template(this.model.toJSON()));
            this.$el = this.$el.children();
            this.setElement(this.$el);
            return this;
        }

    });

    var imageResultItemView = Backbone.View.extend({

        template: _.template($('#image-item-template').html() || ''),

        initialize: function () {
            var img = new Image();
            var that = this;
            img.onload = function () {
                that.model.set({ img_width: img.width });
                that.model.set({ img_height: img.height });
                that.render();
            };
            img.src = this.model.get('ImageUrl');
        },

        render: function () {
            this.$el.html(this.template(this.model.toJSON()));
            this.$el = this.$el.children();
            this.setElement(this.$el);
            return this;
        }

    });

    $(function () {
        if ($("#category-picker").length > 0) {
            //---  Category Picker  ----
            var picker$el = $("#category-picker");
            var controlId = picker$el.data('category-id');
            DR.CategoryPicker = new DR.PickerView({
                el: '#category-picker',
                selectedANodeCallback: function (element) {
                    var id = element.attr('id');
                    $('input[name="CategoryId"]').val(id);
                    window.opener.DR.CategoryControlManager.CategorySelected({
                        ControlID: controlId,
                        Category: {
                            ID: element.data('category-id'),
                            Name: element.data('category-name'),
                            Url: element.data('category-url'),
                            ImageUrl: element.data('category-image-url'),
                            Description: element.data('category-description')
                        }
                    }
                    );
                    window.close(false);
                }
            });
        }

        if ($('#content-picker').length > 0) {
            //---  Content Picker  ---
            var picker$el = $("#content-picker");
            var controlId = picker$el.data('control-id');
            DR.ContentPicker = new DR.PickerView({
                el: '#content-picker',
                selectedANodeCallback: function (element) {
                    window.opener.DR.ContentControlManager.ContentSelected({
                        ControlID: controlId,
                        Content: {
                            Title: element.data('content-title'),
                            Url: element.data('content-url'),
                            Name: element.data('content-name')
                        }
                    }
                    );
                    window.close(false);
                }
            });
        }

        if ($('#image-picker').length > 0) {
            DR.ImagePicker = new ImagePickerPopupView({
                el: '#image-picker'
            });
        }
    });

})(DR || (DR = {}));