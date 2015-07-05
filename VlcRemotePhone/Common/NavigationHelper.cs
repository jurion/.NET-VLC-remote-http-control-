using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VlcRemotePhone.Common
{
    /// <summary>
    /// NavigationHelper aide à naviguer entre les pages.  Il fournit les commandes utilisées pour 
    /// naviguer vers l'arrière et l'avant ainsi que des registres pour les raccourcis de souris et de clavier 
    /// utilisés pour se déplacer vers l’avant ou vers l’arrière dans Windows et le bouton Retour physique dans
    /// Windows Phone. De plus, il intègre SuspensionManger pour assurer la gestion de la durée de vie du processus
    /// et la gestion de l’état lors de la navigation entre pages.
    /// </summary>
    /// <example>
    /// Pour utiliser NavigationHelper, suivez les deux étapes ci-dessous ou
    /// commencer par BasicPage ou un autre modèle d'élément Page différent de BlankPage.
    /// 
    /// 1) Créer une instance de NavigationHelper à un emplacement tel que 
    ///     constructeur de la page et inscrire un rappel pour LoadState et 
    ///     événements SaveState.
    /// <code>
    ///     public MyPage()
    ///     {
    ///         this.InitializeComponent();
    ///         var navigationHelper = new NavigationHelper(this);
    ///         this.navigationHelper.LoadState += navigationHelper_LoadState;
    ///         this.navigationHelper.SaveState += navigationHelper_SaveState;
    ///     }
    ///     
    ///     private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    ///     { }
    ///     private async void navigationHelper_SaveState(object sender, LoadStateEventArgs e)
    ///     { }
    /// </code>
    /// 
    /// 2) Inscrire la page pour appeler NavigationHelper lorsque la page participe 
    ///     à la navigation en remplaçant <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedTo"/> 
    ///     et des événements <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedFrom"/>.
    /// <code>
    ///     protected override void OnNavigatedTo(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedTo(e);
    ///     }
    ///     
    ///     protected override void OnNavigatedFrom(NavigationEventArgs e)
    ///     {
    ///         navigationHelper.OnNavigatedFrom(e);
    ///     }
    /// </code>
    /// </example>
    [Windows.Foundation.Metadata.WebHostHidden]
    public class NavigationHelper : DependencyObject
    {
        private Page Page { get; set; }
        private Frame Frame { get { return this.Page.Frame; } }

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="NavigationHelper"/>.
        /// </summary>
        /// <param name="page">Référence à la page active utilisée pour la navigation.  
        /// Cette référence permet de manipuler le frame et de garantir que le clavier 
        /// les requêtes de navigation ont lieu uniquement lorsque la page occupe toute la fenêtre.</param>
        public NavigationHelper(Page page)
        {
            this.Page = page;

            // Lorsque cette page fait partie de l'arborescence d'éléments visuels, effectue deux modifications :
            // 1) Mappe l'état d'affichage de l'application à l'état visuel pour la page
            // 2) Gérer les raccourcis de demande de navigation physique
            this.Page.Loaded += (sender, e) =>
            {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#else
                // La navigation à l'aide du clavier et de la souris s'applique uniquement lorsque la totalité de la fenêtre est occupée
                if (this.Page.ActualHeight == Window.Current.Bounds.Height &&
                    this.Page.ActualWidth == Window.Current.Bounds.Width)
                {
                    // Écoute directement la fenêtre, ce qui ne requiert pas le focus
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        this.CoreWindow_PointerPressed;
                }
#endif
            };

            // Annule les mêmes modifications lorsque la page n'est plus visible
            this.Page.Unloaded += (sender, e) =>
            {
#if WINDOWS_PHONE_APP
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#else
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    this.CoreWindow_PointerPressed;
#endif
            };
        }

        #region Prise en charge de la navigation

        RelayCommand _goBackCommand;
        RelayCommand _goForwardCommand;

        /// <summary>
        /// <see cref="RelayCommand"/> utilisée pour la liaison à la propriété Command du bouton Précédent
        /// pour accéder à l'élément le plus récent de l'historique de navigation vers l'arrière, si un frame
        /// gère son propre historique de navigation.
        /// 
        /// La commande<see cref="RelayCommand"/> est configurée pour utiliser la méthode virtuelle <see cref="GoBack"/>
        /// comme action d'exécution et <see cref="CanGoBack"/> pour CanExecute.
        /// </summary>
        public RelayCommand GoBackCommand
        {
            get
            {
                if (_goBackCommand == null)
                {
                    _goBackCommand = new RelayCommand(
                        () => this.GoBack(),
                        () => this.CanGoBack());
                }
                return _goBackCommand;
            }
            set
            {
                _goBackCommand = value;
            }
        }
        /// <summary>
        /// <see cref="RelayCommand"/> utilisée pour accéder à l'élément le plus récent du 
        /// l'historique de navigation avant, si un frame gère son propre historique de navigation.
        /// 
        /// La commande<see cref="RelayCommand"/> est configurée pour utiliser la méthode virtuelle <see cref="GoForward"/>
        /// comme action d'exécution et <see cref="CanGoForward"/> pour CanExecute.
        /// </summary>
        public RelayCommand GoForwardCommand
        {
            get
            {
                if (_goForwardCommand == null)
                {
                    _goForwardCommand = new RelayCommand(
                        () => this.GoForward(),
                        () => this.CanGoForward());
                }
                return _goForwardCommand;
            }
        }

        /// <summary>
        /// Méthode virtuelle utilisée par la propriété <see cref="GoBackCommand"/>
        /// pour déterminer si le <see cref="Frame"/> peut reculer.
        /// </summary>
        /// <returns>
        /// true si le <see cref="Frame"/> possède au moins une entrée 
        /// dans l'historique de navigation vers l'arrière.
        /// </returns>
        public virtual bool CanGoBack()
        {
            return this.Frame != null && this.Frame.CanGoBack;
        }
        /// <summary>
        /// Méthode virtuelle utilisée par la propriété <see cref="GoForwardCommand"/>
        /// pour déterminer si le <see cref="Frame"/> peut avancer.
        /// </summary>
        /// <returns>
        /// true si le <see cref="Frame"/> possède au moins une entrée 
        /// dans l'historique de navigation vers l'avant.
        /// </returns>
        public virtual bool CanGoForward()
        {
            return this.Frame != null && this.Frame.CanGoForward;
        }

        /// <summary>
        /// Méthode virtuelle utilisée par la propriété <see cref="GoBackCommand"/>
        /// pour appeler la méthode <see cref="Windows.UI.Xaml.Controls.Frame.GoBack"/>.
        /// </summary>
        public virtual void GoBack()
        {
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }
        /// <summary>
        /// Méthode virtuelle utilisée par la propriété <see cref="GoForwardCommand"/>
        /// pour appeler la méthode <see cref="Windows.UI.Xaml.Controls.Frame.GoForward"/>.
        /// </summary>
        public virtual void GoForward()
        {
            if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Appelé en cas d’appui sur le bouton Retour physique. Pour Windows Phone uniquement.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.GoBackCommand.CanExecute(null))
            {
                e.Handled = true;
                this.GoBackCommand.Execute(null);
            }
        }
#else
        /// <summary>
        /// Invoqué à chaque séquence de touches, notamment les touches système comme les combinaisons utilisant la touche Alt, lorsque
        /// cette page est active et occupe la totalité de la fenêtre.  Utilisé pour détecter la navigation à l'aide du clavier
        /// entre les pages, même lorsque la page elle-même n'a pas le focus.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs e)
        {
            var virtualKey = e.VirtualKey;

            // Approfondit les recherches uniquement lorsque les touches Gauche, Droite ou les touches Précédent et Suivant dédiées
            // sont actionnées
            if ((e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                e.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // Lorsque la touche Précédent ou les touches Alt+Gauche sont actionnées, navigue vers l'arrière
                    e.Handled = true;
                    this.GoBackCommand.Execute(null);
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // Lorsque la touche Suivant ou les touches Alt+Droite sont actionnées, navigue vers l'avant
                    e.Handled = true;
                    this.GoForwardCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Invoqué sur chaque clic de souris, pression d'écran tactile ou interaction équivalente lorsque cette
        /// page est active et occupe la totalité de la fenêtre.  Utilisé pour détecter les clics de souris Suivant et Précédent
        /// de style navigateur pour naviguer entre les pages.
        /// </summary>
        /// <param name="sender">Instance qui a déclenché l'événement.</param>
        /// <param name="e">Données d'événement décrivant les conditions ayant déclenché l'événement.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs e)
        {
            var properties = e.CurrentPoint.Properties;

            // Ignore les pressions simultanées sur les boutons droit, gauche et central
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed)
                return;

            // Si les boutons Précédent ou Suivant sont utilisés (mais pas les deux à la fois) navigue en conséquence
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                e.Handled = true;
                if (backPressed) this.GoBackCommand.Execute(null);
                if (forwardPressed) this.GoForwardCommand.Execute(null);
            }
        }
#endif

        #endregion

        #region Gestion de la durée de vie des processus

        private String _pageKey;

        /// <summary>
        /// Inscrire cet événement sur la page active pour remplir la page
        /// à l'aide du contenu passé lors de la navigation et de tout contenu enregistré
        /// état fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        public event LoadStateEventHandler LoadState;
        /// <summary>
        /// Inscrire cet événement sur la page active pour conserver
        /// l'état associé à la page active en cas de
        /// application est suspendue ou la page est supprimée de
        /// cache de navigation.
        /// </summary>
        public event SaveStateEventHandler SaveState;

        /// <summary>
        /// Appelé lorsque cette page est sur le point d'être affichée dans un frame.  
        /// Cette méthode appelle <see cref="LoadState"/>, où toute la navigation spécifique à la page
        /// et la logique de gestion de durée de vie de processus doivent être placées.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page.  La propriété Parameter
        /// fournit le groupe devant être affiché.</param>
        public void OnNavigatedTo(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // Efface l'état existant pour la navigation avant lors de l'ajout d'une nouvelle page à la
                // pile de navigation
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // Passe le paramètre de navigation à la nouvelle page
                if (this.LoadState != null)
                {
                    this.LoadState(this, new LoadStateEventArgs(e.Parameter, null));
                }
            }
            else
            {
                // Passe le paramètre de navigation et conserve l'état de page de la page, en utilisant
                // la même stratégie pour charger l'état suspendu et recréer les pages supprimées
                // du cache
                if (this.LoadState != null)
                {
                    this.LoadState(this, new LoadStateEventArgs(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]));
                }
            }
        }

        /// <summary>
        /// Invoqué lorsque cette page n'est plus affichée dans un frame.
        /// Cette méthode appelle <see cref="SaveState"/>, où toute la navigation spécifique à la page
        /// et la logique de gestion de durée de vie de processus doivent être placées.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page.  La propriété Parameter
        /// fournit le groupe devant être affiché.</param>
        public void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<String, Object>();
            if (this.SaveState != null)
            {
                this.SaveState(this, new SaveStateEventArgs(pageState));
            }
            frameState[_pageKey] = pageState;
        }

        #endregion
    }

    /// <summary>
    /// Représente la méthode qui gérera l'événement <see cref="NavigationHelper.LoadState"/>
    /// </summary>
    public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
    /// <summary>
    /// Représente la méthode qui gérera l'événement <see cref="NavigationHelper.SaveState"/>
    /// </summary>
    public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

    /// <summary>
    /// Classe utilisée pour contenir les données d'événements requises lorsqu'une page tente de charger l'état.
    /// </summary>
    public class LoadStateEventArgs : EventArgs
    {
        /// <summary>
        /// Valeur de paramètre passée à <see cref="Frame.Navigate(Type, Object)"/> 
        /// lors de la requête initiale de cette page.
        /// </summary>
        public Object NavigationParameter { get; private set; }
        /// <summary>
        /// Dictionnaire d'état conservé par cette page durant une session
        /// antérieure.  Null lors de la première visite de la page.
        /// </summary>
        public Dictionary<string, Object> PageState { get; private set; }

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="LoadStateEventArgs"/>.
        /// </summary>
        /// <param name="navigationParameter">
        /// Valeur de paramètre passée à <see cref="Frame.Navigate(Type, Object)"/> 
        /// lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">
        /// Dictionnaire d'état conservé par cette page durant une session
        /// antérieure.  Null lors de la première visite de la page.
        /// </param>
        public LoadStateEventArgs(Object navigationParameter, Dictionary<string, Object> pageState)
            : base()
        {
            this.NavigationParameter = navigationParameter;
            this.PageState = pageState;
        }
    }
    /// <summary>
    /// Classe utilisée pour contenir les données d'événements requises lorsqu'une page tente d'enregistrer l'état.
    /// </summary>
    public class SaveStateEventArgs : EventArgs
    {
        /// <summary>
        /// Dictionnaire vide à remplir à l'aide de l'état sérialisable.
        /// </summary>
        public Dictionary<string, Object> PageState { get; private set; }

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SaveStateEventArgs"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        public SaveStateEventArgs(Dictionary<string, Object> pageState)
            : base()
        {
            this.PageState = pageState;
        }
    }
}
