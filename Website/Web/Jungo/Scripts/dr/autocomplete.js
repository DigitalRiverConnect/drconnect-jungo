/// <reference path="~/Scripts/jquery-1.8.3-vsdoc.js"/>
/// <reference path="~/Scripts/underscore-min.js" />
/// <reference path="~/Scripts/backbone-min.js" />

var DR;

(function (DR) {
        
    function padNumber(n, p, c) {
        var padChar = typeof c !== 'undefined' ? c : '0';
        var pad = new Array(1 + p).join(padChar);
        return (pad + n).slice(-pad.length);
    }

    var autocompleteService = function (serviceUrl, productUrl, onDataLoaded) {
        this.serviceUrl = serviceUrl;
        this.productUrl = productUrl;
        this.dataLoaded = onDataLoaded;
        this.isInitialized = false;
        this.autoCompleteDictionary = {};
        this.keys = [];

        this.initialize();
    };

    autocompleteService.prototype.initialize = function () {
        var that = this;
        if (this.serviceUrl !== null) {
            var options = {
                url: this.serviceUrl,
                dataType: 'json',
                cache: true,
                success: function (result, message, jqxhr) {
                    that.parseAutocompleteData(result);
                    that.isInitialized = true;
                },
                error: function (jqxhr, status, errorMessge) {
                    // Do something;
                    var m = errorMessge;
                }
            };

            $.ajax(options);
        }
    };

    autocompleteService.prototype.parseAutocompleteData = function (data) {
        _.each(data, function(product) {
            var lcdn = product.DisplayName.toLowerCase();
            var productNameParts = lcdn.split(' ');

            _.each(productNameParts, function(currentProductNamePart) {
                if (typeof this.autoCompleteDictionary[currentProductNamePart] === 'undefined') {
                    this.autoCompleteDictionary[currentProductNamePart] = [];
                    this.keys.push(currentProductNamePart);
                }
                var matches = this.autoCompleteDictionary[currentProductNamePart];
                if (lcdn.indexOf(currentProductNamePart) === 0)
                    matches.splice(0, 0, product);
                else
                    matches.push(product);
            }, this);
        }, this);
        
        this.isInitialized = true;
        if (this.dataLoaded) {
            this.dataLoaded();
        }
    };

    autocompleteService.prototype.getAutocompleteSuggestions = function (keywords, maxResults) {
        var kw = $.trim(keywords.toLowerCase()).split(' ');
        var kwWordsOnly = _.filter(kw, function(item) { return _.isNaN(parseInt(item)); });
        var kwNums = _.filter(kw, function(item) { return !_.isNaN(parseInt(item)); });
        var kwFullWord = _.map(kwWordsOnly, function(item) { return '\\b' + item + '\\b'; });
        var kwFullNumber = _.map(kwNums, function(item) { return '\\b' + item + '\\b'; });
        var keywordRegexPartial = new RegExp(_.map(kw, function(item) { return '\\b' + item; }).join('|'), 'gi');
        var keywordRegexWordsOnlyPartial = new RegExp(_.map(kwWordsOnly, function(item) { return '\\b' + item; }).join('|'), 'gi');
        var keywordRegexNumberOnlyPartial = new RegExp(_.map(kwNums, function(item) { return '\\b' + item; }).join('|'), 'gi');
        var keywordRegexFullWord = new RegExp(kwFullWord.join('|'), 'gi');
        var keywordRegexFullNumber = new RegExp(kwFullNumber.join('|'), 'gi');
        var mr = maxResults || 5;
        var results = [];

        _.each(kw, function(currentKeyword) {
            var matchedKeys = _.filter(this.keys, function (key) {
                return key.lastIndexOf(currentKeyword, 0) === 0;
            });

            _.each(matchedKeys, function(matchKey) {
                var matches = this.autoCompleteDictionary[matchKey];

                _.each(matches, function(currentMatch) {
                    var lcdn = currentMatch.DisplayName.toLowerCase();
                    var fullWordMatches = lcdn.match(keywordRegexFullWord) || [];
                    var fullNumberMatches = lcdn.match(keywordRegexFullNumber) || [];
                    var partialWordMatches = lcdn.match(keywordRegexWordsOnlyPartial) || [];
                    var partialNumberMatches = lcdn.match(keywordRegexNumberOnlyPartial) || [];
                    
                    if (!_.any(results, function(item) { return item.productID === currentMatch.Id; })) {
                        results.push({
                            'productID': currentMatch.Id,
                            'productUrl': this.productUrl.concat('/').concat(currentMatch.Id),
                            'displayName': currentMatch.DisplayName,
                            'displayNameHighlighted': currentMatch.DisplayName.replace(keywordRegexPartial, function (s) { return s.bold(); }),
                            'image': currentMatch.ThumbnailImage,
                            'sort': '' + padNumber((100 - fullWordMatches.length), 3) 
                                + '-' + padNumber((100 - partialWordMatches.length), 3) 
                                + '-' + padNumber((100 - (fullNumberMatches.length > 0 ? 1 : 0)), 3) 
                                + '-' + padNumber((100 - (partialNumberMatches.length > 0 ? 1 : 0)), 3)
                                + '-' + padNumber(lcdn.indexOf(kw[0]), 3)
                                + '-' + lcdn
                        });
                    }
                }, this);
            }, this);
        }, this);
        
        return _.take(_.sortBy(results, 'sort'), mr);
    };

    var searchBoxView = Backbone.View.extend({
        events: {
            'input .input_text' : 'searchBoxChanged',
            'click .ac-container a': 'searchResultSelected',
            'mouseenter .ac-container li': 'mouseHoverEvent',
            'mouseleave .ac-container li': 'mouseHoverEvent'
        },
                
        initialize: function() {
            this.model = new Backbone.Collection();
            this.$searchBox = this.$el.find('.input_text');
            this.$acWrapper = this.$el.find('.ac-wrapper');
            this.$acDropdown = this.$el.find('.ac-container');
            
            this.productUrl = this.$el.data('product-url');
            this.serviceUrl = this.$el.data('service-url');
            this.maxResults = this.$el.data('max-results');
            this.autocompleteService = new autocompleteService(this.serviceUrl, this.productUrl, $.proxy(this.autoCompleteLoaded, this));
            this.autoCompletetItemViews = [];
            this.selectedItem = -1;

            this.listenTo(this.model, 'reset', this.updateAutoCompleteDisplay);

            var that = this;
            $(':not(".ac-container a")').click(function () {
                that.$acWrapper.hide();
            });
            // ie8 fixes
            this.$el.find('.input_text').on('propertychange', function (event) {
                that.searchBoxChanged(event);
            });
            this.$el.find('.input_text').on('keydown', function (event) {
                that.searchBoxKeyDown(event);
            });
        },
        
        mouseHoverEvent: function (elm) {
            this.$el.find('.ac-container li').removeClass('selected');
            $(elm.currentTarget).toggleClass('selected');
        },
        
        searchResultSelected: function (elm) {
            var text = $(elm.currentTarget).find('.product-name').html();
            // strip out any HTML in the string
            text = text.replace(/<\/?[^>]+>/gi, '');
            this.$el.find('.input_text').val(text);
        },
        
        stripTags: function () {
            return this;
        },
        
        searchBoxKeyDown: function(e) {
            if (e.keyCode === 13) {
                if (this.selectedItem !== -1) {
                    e.preventDefault();
                    var selectedView = this.autoCompletetItemViews[this.selectedItem];
                    this.$searchBox.val(selectedView.model.get('displayName'));
                    selectedView.onViewSelected();
                    this.$acWrapper.hide();
                    return false;
                }
                return;
            }
            
            if (e.keyCode === 40) {
                // Down arrow
                this.selectedItem++;
                if (this.selectedItem >= this.autoCompletetItemViews.length)
                    this.selectedItem = -1;

                this.updateSelectedItem();
                return;
            }
            
            if (e.keyCode === 38) {
                // Up Arrow
                this.selectedItem--;
                if (this.selectedItem < -1)
                    this.selectedItem = this.autoCompletetItemViews.length - 1;
                this.updateSelectedItem();
                return;
            }
            
            if (e.keyCode === 27) {
                this.$acWrapper.hide();
                return;
            }
        },
        
        updateSelectedItem: function() {
            _.each(this.autoCompletetItemViews, function(item) {
                item.deselectItem();
            });

            this.autoCompletetItemViews[this.selectedItem].selectItem();
        },
        
        searchBoxChanged: function() {
            if (this.$searchBox.val() === '') {
                this.$acWrapper.hide();
                return;
            }
            
            if (this.autocompleteService.isInitialized) {
                var autoCorrectList = this.autocompleteService.getAutocompleteSuggestions(this.$searchBox.val(), this.maxResults);
                this.model.reset(autoCorrectList);
                this.selectedItem = -1;
                this.updateAutoCompleteDisplay();
            } else {
                this.lastSearch = this.$searchBox.val();
            }
        },
        
        updateAutoCompleteDisplay: function() {
            if (this.model.length === 0) {
                this.$acWrapper.hide();
            } else {
                this.$acWrapper.show();
                this.$acDropdown.empty();
                this.autoCompletetItemViews = [];
                this.model.each(function(item) {
                    var view = new AutoCorrectItemView({
                        model: item,
                        productUrl: this.productUrl
                    });
                    this.autoCompletetItemViews.push(view);
                    this.$acDropdown.append(view.render().el);
                }, this);

                var itemHeightTotal = _.reduce(this.autoCompletetItemViews, function(memo, item) { return memo + item.$el.outerHeight(); }, 0);

                this.$acDropdown.height(itemHeightTotal);
            }
        },
        
        autoCompleteLoaded: function() {
            if (this.lastSearch) {
                this.searchBoxChanged();
            }
        }

    });

    var AutoCorrectItemView = Backbone.View.extend({
       
        tagName: 'li',

        template: _.template($('#ac-item-template').html() || ''),
        
        initialize: function() {
            this.id = 'product-' + this.model.get('productID');
            this.image = new Image();
            $(this.image).on('load', $.proxy(this.imageLoaded, this));
            if(this.model.has('msSmall')) {
                this.image.src = this.model.get('msSmall');    
            }
        },
        
        imageLoaded: function() {
            var $img = this.$el.find('img');
            var paddingHeight = Math.round((this.$el.find('.image-container').height() - $img.outerHeight()) / 2);
            this.$el.find('img').css('padding', '' + paddingHeight + 'px 0');
        },
        
        onViewSelected: function() {
            window.location.href = this.options.productUrl.concat('/').concat(this.model.get('productID'));
        },
        
        selectItem: function() {
            this.$el.addClass('selected');
        },
        
        deselectItem: function() {
            this.$el.removeClass('selected');
        },
        
        render: function() {
            this.$el.html(this.template(this.model.toJSON()));
            return this;
        }
    });

    DR.SearchBoxView = searchBoxView;

})(DR || (DR = {}));