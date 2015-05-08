(function ($) {
	"use strict";

	var isDragging = false;
	var instance; /* global this */
	var isAjaxPending = null; /* used to prevent interaction during Ajax calls */
	var isReloading = false; /* used to keep disabled when navigating away from the page */

	window.n2DragDrop = function(urls, messages, context) {
		this.urls = $.extend({
			copy: 'copy.n2.ashx',
			move: 'move.n2.ashx',
			remove: 'delete.n2.ashx',
			create: 'create.n2.ashx',
			update: 'update.n2.ashx',
			editsingle: '/N2/Content/EditSingle.aspx'
		}, urls);
		this.messages = $.extend({
			deleting: 'Do you really want to delete?',
			helper: "Drop on a highlighted area"
		}, messages);
		this.context = context;
		this.init();
	};

	window.n2DragDrop.prototype = {

		init: function () {
			instance = this; // singleton access

			// Esc key action
			$(document).keyup(function(e) {
				if (e.keyCode == 27) { // Esc
					instance.closeEditor();
					instance.stopDragging();
				}
			});

			// prevent loss of editor when leaving page
			window.onbeforeunload = function() {
				if (window.CKEDITOR) {
					for (name in window.CKEDITOR.instances) {
						var cke = window.CKEDITOR.instances[name];
						if (cke.focusManager.hasFocus) {
							return "There are unsaved changes. Press ESC after closing this dialog to leave editor & save.";
						}
					}
				}
			};

			// Show Ajax animation
			$('#cpOverlay')
				.hide()  // hide it initially
				.ajaxStart(function() {
					$(this).show();
				})
				.ajaxStop(function() {
					if (!isReloading) $(this).hide();
				});

			this.makeDraggable();
			$(document.body).addClass("dragDrop");

			//$(document).on('click', '.titleBar a.command', function (e) {
			//	e.preventDefault();
			//	e.stopPropagation();
			//	instance.showDialog($(this).attr('href'));
			//});
			var host = window.location.protocol + "//" + window.location.host + "/";
			$("a").filter(function () { return this.href.indexOf(host) == 0; })
				.filter(function () { return this.parentNode.className.indexOf('control') < 0; })
				.filter(function () { return !this.target || this.target == "_self"; })
				.each(function () {
					var hashIndex = this.href.indexOf("#");
					if (hashIndex >= 0)
						this.href = this.href.substr(0, hashIndex) + ((this.href.indexOf('?') >= 0 ? '&' : '?') + "edit=drag") + this.href.substr(hashIndex);
					else
						this.href += (this.href.indexOf('?') >= 0 ? '&' : '?') + "edit=drag";
				});

			this.makeEditable();
			this.scroll();
		},

		//showDialog: function (href, dialogOptions) {
		//	href += (href.indexOf('?') >= 0 ? '&' : "?") + "modal=true";
		//	if (dialog) dialog.remove();
		//	dialog = $('<div id="editorDialog" />').appendTo(document.body).hide();
		//	var iframe = document.createElement('iframe');
		//	dialog.append(iframe);
		//	iframe.src = href;
		//	$(iframe).load(function () {
		//		var doc = $(iframe.contentWindow.document);
		//		doc.find('#toolbar a.cancel').click(function () {
		//			dialog.dialog('close');
		//		});
		//	});

		//	dialog.dialog($.extend({
		//		modal: true,
		//		width: Math.min(1000, $(window).width() - 50),
		//		height: Math.min(800, $(window).height() - 100),
		//		closeOnEscape: true,
		//		resizable: true
		//	}, dialogOptions));

		//	window.n2ScrollBack = (function (x, y) {
		//		return function () {
		//			// workaround to maintain scroll position
		//			setTimeout(function () { window.scrollTo(x, y); }, 10);
		//		}
		//	})(window.pageXOffset, window.pageYOffset);
		//},

		makeDraggable: function () {
			$('.definition').draggable({
				dragPrevention: 'a,input,textarea,select,img',
				helper: this.makeDragHelper,
				cursorAt: { top: 8, left: 8 },
				scroll: true,
				stop: this.stopDragging,
				start: this.startDragging
			}).data("handler", this);
			$('.zoneItem').draggable({
				handle: "> .titleBar",
				dragPrevention: 'a,input,textarea,select,img',
				helper: this.makeDragHelper,
				cursorAt: { top: 8, left: 8 },
				scroll: true,
				stop: this.stopDragging,
				start: this.startDragging
			}).data("handler", this);
		},

		appendSelection: function (url, command) {
			return url
				+ (url.indexOf("?") >= 0 ? "&" : "?") 
				+ ((command.below) ? (window.n2SelectedQueryKey || "selected") + "=" + command.below : "")
				+ (this.context.isMasterVersion ? "" : "&versionIndex=" + this.context.versionIndex)
				+ (!command.versionKey ? "" : "&versionKey=" + command.versionKey);
		},

		closeEditor: function() {
			document.activeElement.blur();
			if (window.CKEDITOR) {
				for(name in window.CKEDITOR.instances) {
					var cke = window.CKEDITOR.instances[name];
					if (cke.focusManager.hasFocus) {
						cke.focusManager.blur();
					}
				}
			}
		},

		makeEditable: function () {
			instance = this;
			var cpElement = $("#cpCurtain");
			
			// disable all non-n2 links while editing
			$("a").each(function() {
				var $t = $(this);
				var hr = $t[0].href;
				if (hr.toLowerCase().indexOf("/n2/",0) !== -1) { // TODO: should use configured value
					return;
				}
				if (hr == "" || hr == "#" || $t.closest(cpElement).length > 0) { // skip for CP
					return;
				}
				//if (window.console) console.log("disable: " + hr + " " + $t.closest(cpElement).length + " " + $t.html());

				$t.click(function(e) {
					if (window.console) { console.log("clicked " + hr); }

					e.stopPropagation();
					e.preventDefault(); 
				});				
			});
			
			if (window.CKEDITOR) {
				//if (window.console) {console.log("using CKEditor");}

				$(".wysiwyg, .inplaceedit").each(function() {
					var $t = $(this), cmd = instance.getUpdateCmd($t);

					var config = {
						on: {
							blur: function (e) {
								//$t.removeAttr("contenteditable"); // maybe need a function here
								//console.log("blur " + e.editor.name);
								var value = $.trim(e.editor.getData());
								if (cmd.simple) {
									value = $(value).html(); // remove <p> added by CKEDITOR
								}

								if (e.editor.checkDirty() && cmd.value !=value) {
									if (window.console) {
										console.log(cmd.property + " := " + value);
									}
									cmd.value = value;
									
									instance.process(cmd, function(data) {
											if (data.redirect !== undefined && data.redirect !== "#") {
												isReloading = true;
												window.location = data.redirect;
											}
										}
									);
								}
							}
						}
					};
					
					if ($t.hasClass("inplaceedit")) { // simple text
						config.toolbarGroups = [
							{ name: 'clipboard', groups: [ 'clipboard', 'undo' ] },
							{ name: 'editing', groups: [ 'find', 'selection', 'spellchecker' ] }
						];
						cmd.simple = true;
					}

					// make editable on demand
					$t.click(function(e) {
						//var ce = $t.attr("contenteditable");
						if (!$t.ckeditor && !isAjaxPending)
						{
							window.n2SlidingCurtain.fadeOut();
							
							if (instance.ckeditor) {
								if (window.console) {console.log("dispose CKEditor " + CKEDITOR.instances.length );}
							}

							$t.attr("contenteditable", "true");
							instance.ckeditor = $t.ckeditor = CKEDITOR.inline($t[0], config);
							$t.focus();
							$t.attr("title", cmd.title); // restore original title						    
						}
						// make caret position work on initial click
						e.preventDefault();
						e.stopPropagation();
					});
				});
			}

			$(".editable").each(function () {
				var $t = $(this);
				var url = instance.appendSelection(instance.urls.editsingle, { below: $t.attr("data-path") })
					+ "&property=" + $t.attr("data-property")
					+ "&versionKey=" + $t.attr("data-versionKey")
					+ "&returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search)
					+ "&edit=drag";
				
				$(this).dblclick(function () {
					window.location = url;
				}).each(function () {
					if ($(this).closest("a").length > 0)
						$(this).click(function (e) { e.preventDefault(); e.stopPropagation(); });
				});
				$("<a class='editor n2-icon-pencil' href='" + url + "'></a>").appendTo(this);
			});
		},
		scroll: function () {
			var q = window.location.search;
			var index = q.indexOf("&scroll=") + 8;
			if (index < 0)
				return;
			var ampIndex = q.indexOf("&", index);
			var scroll = q.substr(index, (ampIndex < 0 ? q.length : ampIndex) - index);
			setTimeout(function () {
				window.scrollTo(0, scroll);
			}, 10);
		},
		makeDragHelper: function () {
			isDragging = true;
			var $t = $(this);
			var handler = $t.data("handler");
			$(document.body).addClass("dragging");
			var shadow = document.createElement('div');
			$(shadow).addClass("dragShadow")
				.css({ height: Math.min($t.height(), 200), width: $t.width() })
				.text(handler.messages.helper).appendTo("body");
			return shadow;
		},

		makeDropPoints: function (dragged) {
			var type = $(dragged).addClass("dragged").attr("data-type");

			$(".dropZone").each(function () {
				var zone = this;
				var allowed = $(zone).attr("data-allowed") + ",";
				var title = $(zone).attr("title");
				if (allowed.indexOf(type + ",") >= 0) {
					$(zone).append("<div class='dropPoint below'/>");
					$(".zoneItem", zone)
						.not(function () { return $(this).closest(".dropZone")[0] !== zone; })
						.each(function (i) { $(this).before("<div class='dropPoint before' title='" + i + "'/>"); });
				}
				$(".dropPoint", zone).html("<div class='description'>" + title + "</div>");
			});
			$(dragged).next(".dropPoint").remove();
			$(dragged).prev(".dropPoint").remove();
		},

		makeDroppable: function () {
			$(".dropPoint").droppable({
				activeClass: 'droppable-active',
				hoverClass: 'droppable-hover',
				tolerance: 'pointer',
				drop: this.onDrop,
				over: function (e, ui) {
					instance.currentlyOver = this;
					var $t = $(this);
					$t.data("html", $t.html()).data("height", $t.height());
					//$t.html(ui.draggable.html()).css("height", "auto");
					ui.helper.height($t.height()).width($t.width());
				},
				out: function () {
					if (instance.currentlyOver === this) {
						instance.currentlyOver = null;
					}
					var $t = $(this);
					$t.html($t.data("html")).height($t.data("height"));
				}
			});
		},

		// gets the common ajax command context
		getBaseCmd: function ($t) {
			var ix = $t.attr("data-versionIndex"); // fallback to context version if not set
			ix = (ix > 0) ? ix : instance.context.versionIndex; //same as window.n2ddcp.context.versionIndex
			return {		
				item: $t.attr("data-item"),
				versionKey: $t.attr("data-versionKey"),
				versionIndex: ix,
				discriminator: $t.attr("data-type"),
				template: $t.attr("data-template"),
				returnUrl: window.location.href,
			};
		},
		
		// gets the ajax command context for inline editing -> ItemUpdater.cs
		getUpdateCmd: function ($t) {
			return $.extend(instance.getBaseCmd($t), {
				action: "update",			
				property: $t.attr("data-property"),
				//oldValue: $.trim($t.html()),
				value: $.trim($t.html()),
				simple: false,
				title: $t.attr("title")
			});
		},

		onDrop: function (e, ui) {
			if (isDragging) {
				isDragging = false;

				var $droppable = $(this);
				var $draggable = $(ui.draggable);

				var handler = $draggable.data("handler");
				$draggable.html("");
				$droppable.append("<div class='dropping'/>");
				var $dropzone = $droppable.closest(".dropZone");

				var $next = $droppable.filter(".before").next();
				var cmd = $.extend(instance.getBaseCmd($draggable), {				
					before: ($next.attr("data-versionKey") ? "" : $next.attr("data-item")) || "", // data-item may be page+index+key when new part
					beforeSortOrder: $next.attr("data-sortOrder") || "",
					below: $dropzone.attr("data-item"),
					zone: $dropzone.attr("data-zone"),
					returnUrl: window.location.href,
					dropped: true
				});
				if ($dropzone.attr("data-versionIndex")) {
					cmd.belowVersionIndex = $dropzone.attr("data-versionIndex");
					cmd.belowVersionKey = $dropzone.attr("data-versionKey");
				}
				if ($next.attr("data-versionKey")) {
					cmd.beforeVersionKey = $next.attr("data-versionKey");
					cmd.beforeVersionIndex = $next.attr("data-versionIndex");
				}

				cmd.action = cmd.item ? (e.ctrlKey ? "copy" : "move") : "create";

				handler.process(cmd, function (data) {
					isAjaxPending = null;					    
					if (window.console) {
						console.log(cmd.action + " (AJAX) " + JSON.stringify(data));
					}
					//if (data.redirect && cmd.action == "create" && data.dialog !== "no") {
					//	handler.showDialog(data.redirect);
					//} 
					if (data.redirect) {
						isReloading = true;
						window.location = data.redirect;
					} else {
					    window.location.reload();
					}
				});
			}
		},

		stopDragging: function () {
			window.n2SlidingCurtain.fadeIn();
			$(this).html($(this).data("html")); // restore html removed by jquery ui
			$(this).removeClass("dragged");
			$(".dropPoint").remove();
			$(document.body).removeClass("dragging");
			setTimeout(function () { isDragging = false; }, 100);
		},

		startDragging: function () {
			window.n2SlidingCurtain.fadeOut();
			instance.closeEditor();
			$(this).data("html", $(this).html());
			var dragged = this;
			var handler = $(dragged).data("handler");
			handler.makeDropPoints(dragged);
			handler.makeDroppable();

			dragged.dropHandler = function (ctrl) {
				var id = $(this).attr("data-item");
				if (!id)
					return t.createIn(s.id, d);
				else if (ctrl)
					return t.copyTo(s.id, dragged);
				else
					return t.moveTo(s.id, dragged);
			};
		},

		format: function (f, values) {
				console.log("FORMAT: " + JSON.stringify(values));
			for (var key in values) {
				var keyIndex = url.indexOf("{" + key + "}", 0);
				if (keyIndex >= 0)
					f = f.substring(0, keyIndex) + values[key] + f.substring(2 + keyIndex + window.formatKey.length);
			}
			return f;
		},

		process: function (command, success) {


			command.random = Math.random();

			var url = instance.urls[command.action];
			url = instance.appendSelection(url, command);
			isAjaxPending = command;

			if (window.console) {
				console.log(command.action + " >AJAX> " + JSON.stringify(command));
			}

			var reloaded = false;
			$.post(url, command, function (data) {
				if (window.console) {
					console.log(command.action + " <AJAX< " + JSON.stringify(data));
				}
				reloaded = true;
				if (data.error && data.error == 'true') {
					alert(data.message);
				}
				if ($.isFunction(success)) success(data);
				isAjaxPending = null;
			}, "json").fail(function(data) {
				isAjaxPending = null;
				alert("AJAX Error!\nURL: " + url + "\n" + data.message);
			});

			setTimeout(function () {
			    if (!reloaded) {
			        if (window.console) {
			            console.log(cmd.property + " (AJAX Timeout)");
			        }

					window.location.reload();
			    }
			}, 15000);
		}
	};

	var n2 = {
		setupToolbar: function () {
		},
		refreshPreview: function () {
			window.top.location.reload();
		},
		refresh: function () {
			window.top.location.reload();
		}
	};

	window.n2SlidingCurtain = {
		selector: ".sc",
		closedPos: { top: "0px", left: "0px" },
		openPos: { top: "0px", left: "0px" },

		recalculate: function () {
			var $sc = $(this.selector);
			this.closedPos = { top: (33 - $sc.height()) + "px", left: (5 - $sc.width()) + "px" };
			if (!this.isOpen()) $sc.css(this.closedPos);
		},

		isOpen: function () {
			return $.cookie("sc_open") == "true";
		},

		init: function (selector, startsOpen) {
			this.selector = selector;
			var $sc = $(selector);
			var self = this;
			$(function () {
				self.recalculate();
				setTimeout(function () { self.recalculate(); }, 100);
			});

			self.open = function (e) {
				if (e) {
					$sc.animate(self.openPos);
				} else {
					$sc.css(self.openPos);
				}
				$sc.addClass("opened");
				$.cookie("sc_open", "true", { expires: 1 });
			};
			self.close = function (e) {
				if (e) {
					$sc.animate(self.closedPos);
				} else {
					$sc.css(self.closedPos);
				}
				$sc.removeClass("opened");
				$.cookie("sc_open", null);
			};
			self.fadeIn = function () {
				$sc.fadeIn();
			};
			self.fadeOut = function () {
				$sc.fadeOut();
			};

			if (startsOpen) {
				$sc.animate(self.openPos).addClass("opened");
			} else if (this.isOpen()) {
				self.open();
			} else {
				self.close();
			}

			$sc.find(".close").click(self.close);
			$sc.find(".open").click(self.open);
		}
	};

	window.frameInteraction = {
		location: "Organize",
		ready: true,
		getActions: function() {

			function create(commandElement) {
				return {
					Title: $(commandElement).attr('title'),
					Id: commandElement.id,
					Selector: '#' + commandElement.id,
					Href: commandElement.href,
					CssClass: commandElement.className,
					IconClass: $(commandElement).attr('data-icon-class')
				};
			};

			var actions = [];
			var idCounter = 0;
			$('.controlPanel .plugins .control > a').not('.cpView, .cpAdminister, .cpOrganize, .complementary, .authorizedFalse').each(function() {
				if (!this.id)
					this.id = "action" + ++idCounter;
				actions.push({ Current: create(this) });
			});

			if (actions.length == 0)
				return actions;
			return [
				{
					Current: actions[0].Current,
					Children: actions.slice(1)
				}
			];
		},
		hideToolbar: function() {
			$('.controlPanel .plugins .control > a').not('.cpView, .cpAdminister, .cpOrganize, .complementary, .authorizedFalse')
				.parent().hide();
		},
		execute: function(selector) {
			window.location = $(selector).attr('href');
		}
	};

})(jQuery);
