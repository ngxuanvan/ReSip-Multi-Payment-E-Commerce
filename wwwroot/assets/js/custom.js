console.log("CUSTOM JS LOADED");

(function ($) {

    "use strict";

    var mainWindow          = $(window),
        mainDocument        = $(document),
        youtubeThumb        = $('.youtube-thumbnail'),
        productCarousel     = $('.product-carousel'),
        bxMainSlider        = $('.bxslider'),
        prodSlider          = $('.prod-slider'),
        scrollUp            = $('.scrollup'),
        popup               = $('.popup'),
        ratingSection       = $('.rating-section'),
        bootstrapTouchSlider = $('#bootstrap-touch-slider'),
        testimonialCarousel = $('.testimonial-carousel');

    mainWindow.on('load', function() {

        $('#status').fadeOut();
            $('#preloader').delay(350).fadeOut('slow');
            $('body').delay(350).css({
            'overflow': 'visible'
        });

        youtubeThumb.magnificPopup({
            disableOn: 700,
            type: 'iframe',
            mainClass: 'mfp-fade',
            removalDelay: 160,
            preloader: false,
            fixedContentPos: false
        });

        // Carousel - Product
        $('.featured-carousel').owlCarousel({
            loop: true,
            margin: 24,
            nav: true,
            dots: false,
            responsive: {
                0: { items: 1 },
                576: { items: 2 },
                768: { items: 3 },
                992: { items: 4 }
            }
        });


        testimonialCarousel.owlCarousel({
            loop: true,
            autoplay: true,
            margin: 15,
            dots: false,
            animateIn: true,
            responsiveClass: true,
            navText: [
                '<i class="fa fa-angle-left"></i>',
                '<i class="fa fa-angle-right"></i>'
            ],
            responsive:{
                0:{
                    items:1,
                    nav:true
                },
                600:{
                    items:1,
                    nav:true
                },
                1000:{
                    items:1,
                    nav:true,
                    loop:true
                }
            }
        });
        $(".product-carousel").owlCarousel({
            loop: true,
            margin: 20, // 👈 CHỈ ĐỂ Ở ĐÂY
            nav: true,
            dots: false,
            responsive: {
                0: { items: 1 },
                600: { items: 2 },
                1000: { items: 4 }
            }
        });

        // Scroll to Top
        mainWindow.on("scroll", function() {
            if ($(this).scrollTop() > 98){
                scrollUp.show();
            }
            else{
                scrollUp.hide();
            }
        });

        // Click event to scroll to top
        scrollUp.on("click", function() {
            $('html, body').animate({
                scrollTop: 0
            }, 800);
            return false;
        });

    });
    $(document).ready(function () {
        // Khởi tạo cho Sản phẩm mới nhất
        var owl = $('.featured-carousel');
        if (owl.length > 0) {
            owl.owlCarousel({
                loop: true,
                margin: 20,
                nav: true,
                dots: false,
                autoplay: true,
                autoplayTimeout: 3000,
                responsive: {
                    0: { items: 1 },
                    768: { items: 3 },
                    1200: { items: 4 }
                }
            });
        }

        // Giữ nguyên đoạn bxSlider của bạn nhưng nên đưa vào đây
        if (typeof bxMainSlider !== 'undefined') {
            bxMainSlider.bxSlider({
                auto: true,
                autoControls: true,
                // ... các cấu hình cũ của bạn
            });
        }
    });
    mainDocument.ready(function(){
        bxMainSlider.bxSlider({
            auto: true,
            autoControls: true,
            useCSS: false,
            pager: false,
            mode: 'fade',
            nextText: '<i class="fa fa-chevron-circle-right" aria-hidden="true"></i>',
            prevText: '<i class="fa fa-chevron-circle-left" aria-hidden="true"></i>'
        });
        prodSlider.bxSlider({
            pagerCustom: '#prod-pager'
        });
        popup.magnificPopup({
            type: 'image',
            gallery: {
                enabled: true
            },
        });
        ratingSection.rating();
    });

    bootstrapTouchSlider.bsTouchSlider();

    !function ($) {
        
        $(document).on("click","#left ul.nav li.parent > a > span.sign", function(){          
            $(this).find('i:first').toggleClass("fa-minus");
        }); 
        
        $("#left ul.nav li.parent.active > a > span.sign").find('i:first').addClass("fa-minus");
        $("#left ul.nav li.current").parents('ul.children').addClass("in");

    }(window.jQuery);

    $(".select2").select2();

})(jQuery);