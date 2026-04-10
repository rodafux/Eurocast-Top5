using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Top5.Services;
using Top5.ViewModels;

namespace Top5.Models
{
    public class ShiftReport : ViewModelBase
    {
        private ControlState _rxState = ControlState.NonRenseigne;
        private ControlState _dimensionalState = ControlState.NonRenseigne;
        private ControlState _aspectState = ControlState.NonRenseigne;
        private string _generalComment = string.Empty;
        private int _ancCount = 0;
        private bool _isEditable = true;

        public DateTime WorkDate { get; set; }
        public ShiftType Shift { get; set; }
        public ProductionContext Production { get; set; } = new ProductionContext();

        public Func<string>? GetControllerName { get; set; }

        public bool IsEditable
        {
            get => _isEditable;
            set { _isEditable = value; OnPropertyChanged(); }
        }

        public ControlState RXState
        {
            get => _rxState;
            set { _rxState = value; OnPropertyChanged(); }
        }

        public ControlState DimensionalState
        {
            get => _dimensionalState;
            set { _dimensionalState = value; OnPropertyChanged(); }
        }

        public ControlState AspectState
        {
            get => _aspectState;
            set { _aspectState = value; OnPropertyChanged(); }
        }

        public string GeneralComment
        {
            get => _generalComment;
            set { _generalComment = value; OnPropertyChanged(); }
        }

        public int AncCount
        {
            get => _ancCount;
            set { _ancCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Defect> Defects { get; set; } = new ObservableCollection<Defect>();

        public ICommand CycleStateCommand { get; }
        public ICommand AddDefectCommand { get; }
        public ICommand EditDefectCommand { get; }
        public ICommand IncrementANCCommand { get; }
        public ICommand DecrementANCCommand { get; }

        public ShiftReport()
        {
            CycleStateCommand = new RelayCommand(ExecuteCycleState);
            AddDefectCommand = new RelayCommand(ExecuteAddDefect);
            EditDefectCommand = new RelayCommand(ExecuteEditDefect);

            IncrementANCCommand = new RelayCommand(_ => { if (IsEditable) AncCount++; });
            DecrementANCCommand = new RelayCommand(_ => { if (IsEditable && AncCount > 0) AncCount--; });
        }

        private void ExecuteCycleState(object? parameter)
        {
            if (!IsEditable) return;

            if (parameter is string type)
            {
                if (type == "RX") RXState = GetNextState(RXState);
                else if (type == "3D") DimensionalState = GetNextState(DimensionalState);
                else if (type == "AC") AspectState = GetNextState(AspectState);
            }
        }

        private ControlState GetNextState(ControlState currentState)
        {
            return currentState switch
            {
                ControlState.NonRenseigne => ControlState.B,
                ControlState.B => ControlState.AA,
                ControlState.AA => ControlState.NC,
                ControlState.NC => ControlState.NonRenseigne,
                _ => ControlState.NonRenseigne
            };
        }

        private void ExecuteAddDefect(object? obj)
        {
            if (!IsEditable) return;

            if (Production.Piece == "---" || Production.Moule == "---")
            {
                MessageBox.Show("Impossible d'ajouter un défaut : Aucune pièce ou moule n'est affecté à cette machine actuellement.\nVeuillez d'abord configurer la production en cours.",
                                "Machine sans production", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string controller = GetControllerName?.Invoke() ?? "Inconnu";

            var result = DialogService.Instance.ShowDefectDialog(null, Production, controller, false);

            if (result.Validated && result.Data != null)
            {
                Defects.Add(result.Data);
                DefectHistoryService.LogDefectAction(Production, controller, result.Data, "Création");
            }
        }

        private void ExecuteEditDefect(object? parameter)
        {
            if (parameter is Defect defectToEdit)
            {
                // En mode édition, on bloque si pas de prod. En consultation, on laisse passer.
                if (IsEditable && (Production.Piece == "---" || Production.Moule == "---"))
                {
                    MessageBox.Show("Impossible de modifier un défaut sur une machine sans production affectée.",
                                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string controller = GetControllerName?.Invoke() ?? "Inconnu";

                // Le paramètre !IsEditable force le mode "Lecture Seule" de la modale si l'équipe est clôturée
                var result = DialogService.Instance.ShowDefectDialog(defectToEdit, Production, controller, !IsEditable);

                if (result.Validated && IsEditable)
                {
                    if (result.Deleted)
                    {
                        Defects.Remove(defectToEdit);
                        DefectHistoryService.LogDefectAction(Production, controller, defectToEdit, "Suppression");
                    }
                    else if (result.Data != null)
                    {
                        defectToEdit.DefectType = result.Data.DefectType;
                        defectToEdit.State = result.Data.State;
                        defectToEdit.Comment = result.Data.Comment;
                        defectToEdit.CoreNumber = result.Data.CoreNumber;
                        defectToEdit.IsModified = true;

                        DefectHistoryService.LogDefectAction(Production, controller, defectToEdit, "Modification");
                    }
                }
            }
        }
    }
}