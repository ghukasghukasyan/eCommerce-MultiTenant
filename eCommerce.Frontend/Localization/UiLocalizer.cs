using ClientLibrary.Helpers.Interface;

namespace eCommerce.Frontend.Localization
{
    public class UiLocalizer(ILanguageService languageService)
    {
        private static readonly Dictionary<string, (string En, string Hy, string Ru)> Translations = new()
        {
            ["language"] = ("Language", "Լեզու", "Язык"),
            ["shop"] = ("SHOP", "ՇՈՓ", "МАГАЗИН"),
            ["search_products"] = ("Search products...", "Որոնել ապրանքներ...", "Поиск товаров..."),
            ["no_products_found"] = ("No products found", "Ապրանքներ չեն գտնվել", "Товары не найдены"),
            ["recommended"] = ("Recommended", "Առաջարկվող", "Рекомендуем"),
            ["my_account"] = ("My Account", "Իմ էջը", "Мой аккаунт"),
            ["orders"] = ("Orders", "Պատվերներ", "Заказы"),
            ["logout"] = ("Logout", "Դուրս գալ", "Выйти"),
            ["login"] = ("Login", "Մուտք", "Войти"),
            ["register"] = ("Register", "Գրանցվել", "Регистрация"),
            ["hi"] = ("Hi", "Բարև", "Привет"),
            ["create_account"] = ("Create Account", "Ստեղծել հաշիվ", "Создать аккаунт"),
            ["customer"] = ("Customer", "Հաճախորդ", "Покупатель"),
            ["influencer"] = ("Influencer", "Ինֆլուենսեր", "Инфлюенсер"),
            ["email"] = ("Email", "Էլ. հասցե", "Email"),
            ["full_name"] = ("Full Name", "Անուն Ազգանուն", "Полное имя"),
            ["phone_number"] = ("Phone Number", "Հեռախոսահամար", "Номер телефона"),
            ["password"] = ("Password", "Գաղտնաբառ", "Пароль"),
            ["confirm_password"] = ("Confirm Password", "Հաստատել գաղտնաբառը", "Подтвердите пароль"),
            ["influencer_details"] = ("Influencer details", "Ինֆլուենսերի տվյալներ", "Данные инфлюенсера"),
            ["instagram_url"] = ("Instagram URL", "Instagram հղում", "Ссылка Instagram"),
            ["tiktok_url"] = ("TikTok URL", "TikTok հղում", "Ссылка TikTok"),
            ["apply_influencer"] = ("Apply as Influencer", "Դիմել որպես ինֆլուենսեր", "Подать заявку как инфлюенсер"),
            ["something_wrong"] = ("Something went wrong", "Ինչ-որ բան սխալ է", "Что-то пошло не так"),
            ["email_exists"] = ("Email already exists", "Էլ. հասցեն արդեն գոյություն ունի", "Email уже существует"),
            ["welcome_back"] = ("Welcome Back", "Բարի վերադարձ", "С возвращением"),
            ["sign_in_prompt"] = ("Please sign in to your account", "Խնդրում ենք մուտք գործել ձեր հաշիվ", "Пожалуйста, войдите в аккаунт"),
            ["email_address"] = ("Email Address", "Էլ. հասցե", "Email"),
            ["logging_in"] = ("Logging in...", "Մուտք...", "Вход..."),
            ["create_an_account"] = ("Create an account", "Ստեղծել հաշիվ", "Создать аккаунт"),
            ["already_have_account"] = ("Already have an account?", "Արդեն ունե՞ք հաշիվ։", "Уже есть аккаунт?"),

            // Email confirmation
            ["check_your_email"] = ("Check your email", "Ստուգեք ձեր էլ. փոստը", "Проверьте вашу почту"),
            ["confirm_email_msg"] = ("We sent a confirmation link to your email. Please click it to activate your account.", "Մենք հաստատման հղում ենք ուղարկել ձեր էլ. փոստին:", "Мы отправили ссылку для подтверждения на вашу почту."),
            ["go_to_login"] = ("Go to Login", "Անցնել մուտք", "Перейти к входу"),
            ["email_confirmed_title"] = ("Email Confirmed!", "Էլ. փոստը հաստատված է!", "Email подтверждён!"),
            ["email_confirmed_msg"] = ("Your email has been confirmed. You can now log in.", "Ձեր էլ. փոստը հաստատված է: Կարող եք մուտք գործել:", "Ваш email подтверждён. Теперь вы можете войти."),
            ["confirm_failed_title"] = ("Confirmation Failed", "Հաստատումը ձախողվեց", "Подтверждение не удалось"),
            ["invalid_link"] = ("The link is invalid or has expired.", "Հղումն անվավեր է կամ ժամկետն անցել է:", "Ссылка недействительна или истекла."),
            ["invalid_link_title"] = ("Invalid Link", "Անվավեր հղում", "Недействительная ссылка"),

            // Forgot / reset password
            ["forgot_password"] = ("Forgot password?", "Մոռացե՞լ եք գաղտնաբառը", "Забыли пароль?"),
            ["forgot_password_prompt"] = ("Enter your email and we'll send you a reset link.", "Մուտքագրեք ձեր էլ. փոստը, և մենք կուղարկենք վերականգնման հղում:", "Введите email, и мы отправим ссылку для сброса."),
            ["send_reset_link"] = ("Send Reset Link", "Ուղարկել հղում", "Отправить ссылку"),
            ["reset_link_sent_msg"] = ("If that email exists in our system, a reset link has been sent. Check your inbox.", "Եթե այդ էլ. փոստն առկա է, հղումն ուղարկվել է:", "Если email существует, ссылка отправлена. Проверьте почту."),
            ["back_to_login"] = ("Back to Login", "Վերադառնալ մուտք", "Вернуться к входу"),
            ["reset_password"] = ("Reset Password", "Վերականգնել գաղտնաբառը", "Сбросить пароль"),
            ["new_password"] = ("New Password", "Նոր գաղտնաբառ", "Новый пароль"),
            ["password_reset_success"] = ("Password Reset!", "Գաղտնաբառը վերականգնված է!", "Пароль сброшен!"),
            ["password_reset_success_msg"] = ("Your password has been reset. You can now log in with your new password.", "Ձեր գաղտնաբառը վերականգնվել է:", "Ваш пароль сброшен. Войдите с новым паролем."),
            ["passwords_dont_match"] = ("Passwords do not match", "Գաղտնաբառերը չեն համընկնում", "Пароли не совпадают"),
            ["try_again"] = ("Try Again", "Կրկին փորձել", "Попробовать снова"),

            // Common UI actions
            ["loading"] = ("Loading...", "Բեռնվում է...", "Загрузка..."),
            ["saving"] = ("Saving...", "Պահպանվում...", "Сохранение..."),
            ["cancel"] = ("Cancel", "Չեղարկել", "Отмена"),
            ["save"] = ("Save", "Պահպանել", "Сохранить"),
            ["edit"] = ("Edit", "Խմբագրել", "Изменить"),
            ["delete"] = ("Delete", "Ջնջել", "Удалить"),
            ["view"] = ("View", "Դիտել", "Просмотр"),
            ["add_to_cart"] = ("Add to cart", "Ավելացնել զամբյուղ", "В корзину"),
            ["see_more"] = ("See More", "Տեսնել ավելին", "Подробнее"),
            ["view_all"] = ("View All", "Տեսնել բոլորը", "Смотреть все"),
            ["shop_now"] = ("Shop Now", "Գնել հիմա", "Купить сейчас"),
            ["buy_now"] = ("Buy Now", "Գնել", "Купить"),
            ["explore"] = ("Explore", "Ուսումնասիրել", "Изучить"),
            ["new_badge"] = ("New", "Նոր", "Новинка"),
            ["home"] = ("Home", "Գլխավոր", "Главная"),
            ["search"] = ("Search", "Որոնել", "Поиск"),
            ["cart"] = ("Cart", "Զամբյուղ", "Корзина"),
            ["account"] = ("Account", "Հաշիվ", "Аккаунт"),
            ["name"] = ("Name", "Անուն", "Имя"),
            ["phone"] = ("Phone", "Հեռախոս", "Телефон"),
            ["city"] = ("City", "Քաղաք", "Город"),
            ["address_field"] = ("Address", "Հասցե", "Адрес"),
            ["notes_field"] = ("Notes", "Նշումներ", "Примечания"),
            ["date"] = ("Date", "Ամսաթիվ", "Дата"),
            ["status"] = ("Status", "Կարգավիճակ", "Статус"),
            ["actions"] = ("Actions", "Գործողություններ", "Действия"),
            ["price_label"] = ("Price", "Գին", "Цена"),
            ["prev"] = ("← Prev", "← Նախ.", "← Пред."),
            ["next"] = ("Next →", "Հաջ. →", "След. →"),
            ["page_label"] = ("Page", "Էջ", "Страница"),
            ["of_label"] = ("of", "из", "из"),

            // Profile
            ["profile"] = ("Profile", "Պրոֆիլ", "Профиль"),
            ["unknown_user"] = ("Unknown User", "Անհայտ օգտատեր", "Неизвестный пользователь"),
            ["edit_profile"] = ("Edit Profile", "Խմբագրել պրոֆիլը", "Редактировать профиль"),
            ["save_changes"] = ("Save Changes", "Պահպանել փոփոխությունները", "Сохранить изменения"),
            ["loading_profile"] = ("Loading profile...", "Պրոֆիլը բեռնվում է...", "Загрузка профиля..."),

            // Addresses
            ["addresses"] = ("Addresses", "Հասցեներ", "Адреса"),
            ["add_address"] = ("+ Add Address", "+ Ավելացնել հասցե", "+ Добавить адрес"),
            ["no_addresses"] = ("No addresses yet", "Հասցեներ չկան", "Адреса не найдены"),
            ["default_badge"] = ("Default", "Լռելյայն", "По умолчанию"),
            ["set_default"] = ("Set Default", "Կանխադրել", "Сделать основным"),
            ["edit_address"] = ("Edit Address", "Խմբագրել հասցեն", "Изменить адрес"),
            ["add_address_title"] = ("Add Address", "Ավելացնել հասցե", "Добавить адрес"),
            ["set_as_default"] = ("Set as default", "Կանխադրել", "Сделать основным"),
            ["postal_code"] = ("Postal Code", "Փոստային ինդեքս", "Почтовый индекс"),
            ["use_saved_address"] = ("Use a saved address", "Օգտ. պահված հասցե", "Использовать сохр. адрес"),
            ["or_enter_manually"] = ("Or enter manually", "Կամ մուտ. ձեռ.", "Или ввести вручную"),

            // Orders
            ["loading_orders"] = ("Loading orders...", "Պատվերները բեռնվում են...", "Загрузка заказов..."),
            ["no_orders"] = ("You don't have any orders yet.", "Դուք դեռ պատվեր չունեք:", "У вас пока нет заказов."),
            ["order_num"] = ("Order #", "Պատվեր #", "Заказ #"),
            ["loading_order"] = ("Loading order...", "Պատվերը բեռնվում է...", "Загрузка заказа..."),
            ["order_label"] = ("Order", "Պատվեր", "Заказ"),
            ["qty"] = ("Qty", "Քանակ", "Кол-во"),
            ["shipping_details"] = ("Shipping details", "Առաքման տվյալներ", "Данные доставки"),
            ["products_label"] = ("Products", "Ապրանքներ", "Товары"),

            // Brand highlights
            ["natural_ingredients_title"] = ("Natural Ingredients", "Բնական բաղադրիչներ", "Натуральные ингредиенты"),
            ["natural_ingredients_desc"] = ("Carefully sourced botanicals and skin-loving formulas", "Ուշադիր ընտրված բուսական բաղադրիչներ", "Тщательно отобранные растительные компоненты"),
            ["fast_delivery_title"] = ("Fast Delivery", "Արագ առաքում", "Быстрая доставка"),
            ["fast_delivery_desc"] = ("Express shipping to your door within 1–3 days", "Արտահայտ առաքում 1–3 օրվա ընթացքում", "Экспресс-доставка за 1–3 дня"),
            ["derm_tested_title"] = ("Dermatologist Tested", "Ստուգված դերմատոլոգի կողմից", "Протестировано дерматологами"),
            ["derm_tested_desc"] = ("Safe for all skin types, including sensitive skin", "Անվտանգ բոլոր մաշկատեսակների համար", "Безопасно для всех типов кожи"),
            ["loved_by_customers_title"] = ("Loved by Customers", "Հաճախորդների ընտրությունը", "Любимое покупателями"),
            ["loved_by_customers_desc"] = ("Thousands of happy customers across Armenia", "Հազարավոր երջանիկ հաճախորդներ Հայաստանում", "Тысячи довольных покупателей по Армении"),

            // Category showcase
            ["browse_by_category"] = ("Browse By Category", "Դիտել ըստ կատեգորիայի", "Просмотр по категориям"),
            ["shop_what_you_love"] = ("Shop What You Love", "Ընտրեք ձեր ուզածը", "Выбирайте то, что любите"),
            ["explore_collections"] = ("Explore our curated beauty collections", "Ուսումնասիրեք մեր գեղեցկության հավաքածուները", "Исследуйте наши коллекции красоты"),
            ["all_products"] = ("All Products", "Բոլոր ապրանքները", "Все товары"),

            // Featured products
            ["hand_picked"] = ("Hand Picked", "Ձեռքով ընտրված", "Отобрано вручную"),
            ["most_loved_subtitle"] = ("Our most loved products, chosen for you", "Մեր ամենասիրված ապրանքները ձեզ համար", "Наши самые любимые товары для вас"),
            ["no_featured"] = ("No featured products available.", "Ընդգծված ապրանքներ չկան:", "Рекомендуемые товары отсутствуют."),
            ["already_in_cart"] = ("Already in your cart!", "Արդեն զամբյուղում է!", "Уже в корзине!"),
            ["added_to_cart"] = ("Added to cart!", "Ավելացվեց զամբյուղ!", "Добавлено в корзину!"),

            // Editorial banner
            ["our_philosophy"] = ("Our Philosophy", "Մեր Փիլիսոփայությունը", "Наша Философия"),
            ["editorial_title"] = ("Beauty rooted\nin nature.", "Գեղեցկություն,\nծնված բնությունից:", "Красота,\nвдохновлённая природой."),
            ["editorial_subtitle"] = ("Every formula we craft begins with the finest natural ingredients — chosen to nourish, balance, and reveal your most radiant self.", "Յուրաքանչյուր բաղադրություն ստեղծվում է լավագույն բնական բաղադրիչներից:", "Каждая формула создаётся из лучших натуральных ингредиентов."),
            ["discover_more"] = ("Discover More", "Ավելին", "Подробнее"),

            // Hero slides
            ["new_collection"] = ("NEW COLLECTION", "ՆՈՐ ՀԱՎԱՔԱԾՈՒ", "НОВАЯ КОЛЛЕКЦИЯ"),
            ["slide1_title"] = ("Lip & Cheek Essentials", "Շրթներկ & Ռումյաններ", "Губы и щёки"),
            ["slide1_sub"] = ("Glow naturally with soft, buildable tones.", "Բնական փայլ մեղմ երանգներով:", "Естественное сияние с мягкими тонами."),
            ["best_sellers_tag"] = ("BEST SELLERS", "ԱՄԵՆԱՎԱՃԱՌՎԱԾ", "БЕСТСЕЛЛЕРЫ"),
            ["slide2_title"] = ("Soft Matte Beauty", "Փափուկ Մաթ Գեղեցկություն", "Мягкий матовый макияж"),
            ["slide2_sub"] = ("Lightweight formulas that last all day.", "Թեթև բաղադրություններ ամբողջ օրվա համար:", "Лёгкие формулы на весь день."),
            ["limited_edition"] = ("LIMITED EDITION", "ՍԱՀՄԱՆԱՓԱԿ ԹՈՂԱՐԿՈՒՄ", "ОГРАНИЧЕННЫЙ ВЫПУСК"),
            ["slide3_title"] = ("Natural Glow Kit", "Բնական Glow Kit", "Набор натурального сияния"),
            ["slide3_sub"] = ("Minimal makeup, maximum radiance.", "Նվազ մեյքափ, առավ. փայլ:", "Минимум макияжа, максимум сияния."),

            // Payment pages
            ["payment_cancelled"] = ("Payment Canceled", "Վճարումը չեղարկված է", "Платёж отменён"),
            ["payment_success_msg"] = ("Payment Successful! Thank you for your purchase.", "Վճարումը հաջողվեց! Շնորհակալություն գնման համար:", "Платёж успешен! Спасибо за покупку."),

            // Search / latest products
            ["latest_products"] = ("Latest Products", "Վերջին ապրանքներ", "Последние товары"),

            // Components
            ["loading_products"] = ("Loading products...", "Ապրանքները բեռնվում են...", "Загрузка товаров..."),
            ["no_products_category"] = ("No products found in this category.", "Այս կատեգ. ապրանքներ չկան:", "В этой категории нет товаров."),
            ["sort_by"] = ("Sort by", "Դասավ.", "Сортировка"),
            ["products"] = ("products", "ապրանք", "товаров"),
            ["out_of_stock"] = ("Out of stock", "Ապրանքն սպառված է", "Нет в наличии"),
            ["product_unavailable"] = ("Product unavailable", "Ապրանքն անհասանելի է", "Товар недоступен"),
            ["product_added_to_cart"] = ("Product added to cart!", "Ապրանքն ավելացվեց զամ.!", "Товар добавлен в корзину!"),

            // Footer
            ["footer_tagline"] = ("Natural beauty for every skin tone, every day.", "Բնական գեղեցկություն ամեն մաշկի, ամեն օր:", "Натуральная красота для каждого тона кожи, каждый день."),
            ["new_arrivals"] = ("New Arrivals", "Նոր ժամանումներ", "Новинки"),
            ["best_sellers"] = ("Best Sellers", "Ամենավաճառված", "Бестселлеры"),
            ["support"] = ("Support", "Աջակցություն", "Поддержка"),
            ["contact_us"] = ("Contact Us", "Կապ մեզ հետ", "Связаться с нами"),
            ["about_store"] = ("About Us", "Մեր մասին", "О нас"),
            ["privacy_policy"] = ("Privacy Policy", "Գաղ. քաղաքականություն", "Политика конфиденциальности"),
            ["returns_refunds"] = ("Returns & Refunds", "Վերադարձ & Փոխհատ.", "Возврат и возмещение"),
            ["stay_in_touch"] = ("Stay in Touch", "Կապ պահե՛ք մեզ հետ", "Оставайтесь на связи"),
            ["newsletter_sub"] = ("Subscribe for exclusive offers and new arrivals.", "Բաժ. բացառ. առաջ. ու նոր ժամ. համ.:", "Подпишитесь для эксклюзивных предложений."),
            ["email_placeholder"] = ("your@email.com", "ձեր@email.com", "ваш@email.com"),
            ["call_us"] = ("Call us:", "Զ.՛ մեզ:", "Позвоните нам:"),
            ["secure_checkout"] = ("Secure Checkout", "Անվտ. վճարում", "Безопасная оплата"),
            ["designed_by"] = ("Designed by", "Դիզ.՝", "Дизайн:"),

            // Influencer coupons
            ["my_coupons"] = ("My Coupons", "Իմ կուպոնները", "Мои купоны"),
            ["coupon_code"] = ("Code", "Կոդ", "Код"),
            ["discount"] = ("Discount", "Զեղ.", "Скидка"),
            ["commission"] = ("Commission", "Միջնորդ.", "Комиссия"),
            ["expiry"] = ("Expiry", "Ժամ. լ.", "Срок"),
            ["used_count"] = ("Used", "Օգտ.", "Исп."),
            ["no_expiry"] = ("No expiry", "Անժամ.", "Без срока"),
            ["no_coupons"] = ("No coupons assigned yet.", "Կուպոններ դեռ չկան:", "Купоны ещё не назначены."),

            // Cart
            ["my_cart"] = ("My Cart", "Իմ զամբյուղը", "Моя корзина"),
            ["items"] = ("items", "ապրանք", "товаров"),
            ["order_summary"] = ("Order Summary", "Պատվերի ամփոփում", "Сводка заказа"),
            ["subtotal"] = ("Subtotal", "Միջանկյալ", "Промежуточный итог"),
            ["total"] = ("Total", "Ընդամենը", "Итого"),
            ["proceed_to_checkout"] = ("Proceed to Checkout", "Անցնել վճարմանը", "Перейти к оформлению"),
            ["cart_empty"] = ("Your cart is empty", "Ձեր զամբյուղը դատարկ է", "Ваша корзина пуста"),
            ["add_products_prompt"] = ("Add products to continue shopping", "Ավելացրեք ապրանքներ գնումները շարունակելու համար", "Добавьте товары для продолжения покупок"),
            ["continue_shopping"] = ("Continue Shopping", "Շարունակել գնումները", "Продолжить покупки"),

            // Order success
            ["order_placed_successfully"] = ("Order Placed Successfully", "Պատվերն ընդունված է", "Заказ успешно оформлен"),
            ["pay_on_delivery"] = ("You will pay upon delivery", "Վճարումը կատարվելու է առաքման ժամանակ", "Оплата при доставке"),
            ["order_id"] = ("Order ID", "Պատվերի համար", "Номер заказа"),
            ["back_to_shop"] = ("Back to shop", "Վերադառնալ խանութ", "Вернуться в магазин"),
            ["order_confirmed_sub"] = ("We've received your order and will prepare it shortly.", "Մենք ստացել ենք ձեր պատվերը և շուտով կպատրաստենք այն։", "Мы получили ваш заказ и скоро его подготовим.")
        };

        public string this[string key] => Get(key);

        public string Get(string key)
        {
            if (!Translations.TryGetValue(key, out var value))
                return key;

            return languageService.CurrentLanguage switch
            {
                "hy" => value.Hy,
                "ru" => value.Ru,
                _ => value.En
            };
        }
    }
}
