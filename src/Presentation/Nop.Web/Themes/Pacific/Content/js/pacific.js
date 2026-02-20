/**
 * Pacific Theme - NopCommerce 4.90
 * Main JavaScript File
 */

(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initStickyHeader();
        initBackToTop();
        initMobileMenu();
        initProductHover();
        initSearchBox();
    });

    /**
     * Sticky Header
     * Makes header fixed when scrolling past a threshold
     */
    function initStickyHeader() {
        var header = document.querySelector('.pacific-header, .header');
        if (!header) return;

        var headerHeight = header.offsetHeight;
        var scrollThreshold = 100;
        var isSticky = false;

        function handleScroll() {
            var scrollTop = window.pageYOffset || document.documentElement.scrollTop;

            if (scrollTop > scrollThreshold && !isSticky) {
                header.classList.add('is-sticky');
                document.body.style.paddingTop = headerHeight + 'px';
                isSticky = true;
            } else if (scrollTop <= scrollThreshold && isSticky) {
                header.classList.remove('is-sticky');
                document.body.style.paddingTop = '';
                isSticky = false;
            }
        }

        // Throttle scroll event
        var ticking = false;
        window.addEventListener('scroll', function() {
            if (!ticking) {
                window.requestAnimationFrame(function() {
                    handleScroll();
                    ticking = false;
                });
                ticking = true;
            }
        });
    }

    /**
     * Back to Top Button
     * Shows/hides button based on scroll position
     */
    function initBackToTop() {
        // Create back to top button if not exists
        var backToTop = document.querySelector('.pacific-back-to-top');
        if (!backToTop) {
            backToTop = document.createElement('button');
            backToTop.className = 'pacific-back-to-top';
            backToTop.setAttribute('aria-label', 'Back to top');
            backToTop.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M12 4l-8 8h5v8h6v-8h5z"/></svg>';
            document.body.appendChild(backToTop);
        }

        var scrollThreshold = 300;

        function handleScroll() {
            var scrollTop = window.pageYOffset || document.documentElement.scrollTop;

            if (scrollTop > scrollThreshold) {
                backToTop.classList.add('is-visible');
            } else {
                backToTop.classList.remove('is-visible');
            }
        }

        // Throttle scroll event
        var ticking = false;
        window.addEventListener('scroll', function() {
            if (!ticking) {
                window.requestAnimationFrame(function() {
                    handleScroll();
                    ticking = false;
                });
                ticking = true;
            }
        });

        // Scroll to top on click
        backToTop.addEventListener('click', function() {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
    }

    /**
     * Mobile Menu Toggle
     * Handles hamburger menu functionality
     */
    function initMobileMenu() {
        // Main menu toggle in header
        var headerMenuToggle = document.querySelector('.pacific-nav .pacific-menu-toggle');
        var mainMenuToggle = document.querySelector('.pacific-menu-container .pacific-menu-toggle, .menu-container .menu__toggle');
        var menu = document.querySelector('.pacific-menu, .menu');

        // Handle header menu toggle
        if (headerMenuToggle) {
            headerMenuToggle.addEventListener('click', function(e) {
                e.preventDefault();
                if (menu) {
                    menu.classList.toggle('active');
                    headerMenuToggle.classList.toggle('is-active');
                    var isExpanded = menu.classList.contains('active');
                    headerMenuToggle.setAttribute('aria-expanded', isExpanded);
                }
            });
        }

        // Handle main menu toggle (inside menu container)
        if (mainMenuToggle && menu) {
            mainMenuToggle.addEventListener('click', function(e) {
                e.preventDefault();
                menu.classList.toggle('active');
                mainMenuToggle.classList.toggle('is-active');
                var isExpanded = menu.classList.contains('active');
                mainMenuToggle.setAttribute('aria-expanded', isExpanded);
            });
        }

        // Close menu when clicking outside
        document.addEventListener('click', function(e) {
            if (!menu) return;

            var isClickInside = false;

            if (headerMenuToggle && headerMenuToggle.contains(e.target)) isClickInside = true;
            if (mainMenuToggle && mainMenuToggle.contains(e.target)) isClickInside = true;
            if (menu.contains(e.target)) isClickInside = true;

            if (!isClickInside && menu.classList.contains('active')) {
                menu.classList.remove('active');
                if (headerMenuToggle) {
                    headerMenuToggle.classList.remove('is-active');
                    headerMenuToggle.setAttribute('aria-expanded', 'false');
                }
                if (mainMenuToggle) {
                    mainMenuToggle.classList.remove('is-active');
                    mainMenuToggle.setAttribute('aria-expanded', 'false');
                }
            }
        });

        // Handle submenu toggles on mobile
        var menuItems = document.querySelectorAll('.pacific-menu-item.has-children, .menu > li.has-children');
        menuItems.forEach(function(item) {
            var link = item.querySelector('.pacific-menu-link, > a');
            if (link) {
                link.addEventListener('click', function(e) {
                    if (window.innerWidth <= 767) {
                        e.preventDefault();
                        item.classList.toggle('is-expanded');

                        // Toggle visibility of submenu
                        var submenu = item.querySelector('.pacific-dropdown, .pacific-megamenu');
                        if (submenu) {
                            if (item.classList.contains('is-expanded')) {
                                submenu.style.display = 'block';
                            } else {
                                submenu.style.display = 'none';
                            }
                        }
                    }
                });
            }
        });
    }

    /**
     * Product Hover Effects
     * Enhance product card interactions
     */
    function initProductHover() {
        var productCards = document.querySelectorAll('.product-item, .pacific-product-card');

        productCards.forEach(function(card) {
            // Touch device handling
            if ('ontouchstart' in window) {
                card.addEventListener('touchstart', function() {
                    this.classList.add('is-touched');
                });

                card.addEventListener('touchend', function() {
                    var self = this;
                    setTimeout(function() {
                        self.classList.remove('is-touched');
                    }, 300);
                });
            }
        });
    }

    /**
     * Search Box Enhancement
     * Add focus states and animations
     */
    function initSearchBox() {
        var searchForms = document.querySelectorAll('.pacific-search form, .pacific-search-form');

        searchForms.forEach(function(form) {
            var input = form.querySelector('input[type="text"], input[type="search"], .pacific-search-input, .search-box-text');
            if (!input) return;

            input.addEventListener('focus', function() {
                form.classList.add('is-focused');
            });

            input.addEventListener('blur', function() {
                form.classList.remove('is-focused');
            });
        });
    }

    /**
     * Add to Cart Animation
     */
    function addToCartAnimation(button) {
        button.classList.add('adding');
        setTimeout(function() {
            button.classList.remove('adding');
            button.classList.add('added');
            setTimeout(function() {
                button.classList.remove('added');
            }, 2000);
        }, 500);
    }

    // Expose to global scope if needed
    window.PacificTheme = {
        addToCartAnimation: addToCartAnimation
    };

})();
