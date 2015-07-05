using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VlcRemotePhone.Common
{
    /// <summary>
    /// Commande dont le seul objectif est de relayer sa fonctionnalité 
    /// à d'autres objets en appelant des délégués. 
    /// La valeur de retour par défaut pour la méthode CanExecute est 'true'.
    /// <see cref="RaiseCanExecuteChanged"/> doit être appelé lorsque
    /// <see cref="CanExecute"/> est censé retourner une valeur différente.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Déclenché lors de l'appel de RaiseCanExecuteChanged.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Crée une nouvelle commande qui peut toujours s'exécuter.
        /// </summary>
        /// <param name="execute">Logique d'exécution.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Crée une commande.
        /// </summary>
        /// <param name="execute">Logique d'exécution.</param>
        /// <param name="canExecute">Logique d'état d'exécution.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Détermine si cette <see cref="RelayCommand"/> peut être exécutée dans son état actuel.
        /// </summary>
        /// <param name="parameter">
        /// Données utilisées par la commande. Si la commande ne nécessite pas la transmission des données, cet objet peut avoir la valeur Null.
        /// </param>
        /// <returns>true si cette commande peut être exécutée ; sinon, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        /// <summary>
        /// Exécute la commande <see cref="RelayCommand"/> sur la cible actuelle.
        /// </summary>
        /// <param name="parameter">
        /// Données utilisées par la commande. Si la commande ne nécessite pas la transmission des données, cet objet peut avoir la valeur Null.
        /// </param>
        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// Méthode utilisée pour déclencher l'événement <see cref="CanExecuteChanged"/>
        /// pour indiquer que la valeur de retour de <see cref="CanExecute"/>
        /// la méthode a changé.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}