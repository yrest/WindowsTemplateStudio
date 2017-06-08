﻿using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Mvvm;
using Microsoft.Templates.UI.Resources;
using Microsoft.Templates.UI.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Microsoft.Templates.UI.ViewModels.NewItem
{
    public class NewItemSetupViewModel : Observable
    {
        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private Visibility _editionVisibility = Visibility.Collapsed;
        public Visibility EditionVisibility
        {
            get => _editionVisibility;
            set => SetProperty(ref _editionVisibility, value);
        }

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set
            {
                SetProperty(ref _itemName, value);
                UpdateItemName(_itemName);
            }
        }

        private InformationViewModel _information;
        public InformationViewModel Information
        {
            get => _information;
            set => SetProperty(ref _information, value);
        }

        public ObservableCollection<ItemsGroupViewModel<TemplateInfoViewModel>> TemplateGroups { get; } = new ObservableCollection<ItemsGroupViewModel<TemplateInfoViewModel>>();

        public NewItemSetupViewModel()
        {

        }

        public void Initialize()
        {
            var templates = GenContext.ToolBox.Repo.Get(t =>
                                                  (t.GetTemplateType() == TemplateType.Page || t.GetTemplateType() == TemplateType.Feature)
                                                  && t.GetFrameworkList().Contains(MainViewModel.Current.ConfigFramework)
                                                  && t.GetTemplateType() == MainViewModel.Current.ConfigTemplateType
                                                  && t.GetRightClickEnabled())
                                                  .Select(t => new TemplateInfoViewModel(t, GenComposer.GetAllDependencies(t, MainViewModel.Current.ConfigFramework)));


            var groups = templates.GroupBy(t => t.Group).Select(gr => new ItemsGroupViewModel<TemplateInfoViewModel>(gr.Key as string, gr.ToList(), OnSelectedItemChanged)).OrderBy(gr => gr.Title);

            TemplateGroups.Clear();
            TemplateGroups.AddRange(groups);
            if (TemplateGroups.Any())
            {
                var group = TemplateGroups.First();
                group.SelectedItem = group.Templates.First();
                MainViewModel.Current.NoContentVisibility = Visibility.Collapsed;
                MainViewModel.Current.EnableGoForward();
                MainViewModel.Current.SetTemplatesReadyForProjectCreation();
            }
            else
            {
                MainViewModel.Current.NoContentVisibility = Visibility.Visible;
            }
            UpdateHeader(templates.Count());
        }

        private void OnSelectedItemChanged(ItemsGroupViewModel<TemplateInfoViewModel> group)
        {
            foreach (var gr in TemplateGroups)
            {
                if (gr.Name == group.Name)
                {
                    var template = gr.SelectedItem as TemplateInfoViewModel;
                    if (template != null)
                    {
                        UpdateItemName(template);
                        Information = new InformationViewModel(template);
                    }
                }
                else
                {
                    gr.CleanSelected();
                }

            }
        }

        private void UpdateItemName(TemplateInfoViewModel template)
        {
            var validators = new List<Validator>() { new ReservedNamesValidator() };
            if (template.IsItemNameEditable)
            {
                validators.Add(new DefaultNamesValidator());
                EditionVisibility = Visibility.Visible;
            }
            else
            {
                EditionVisibility = Visibility.Collapsed;
            }
            _itemName = Naming.Infer(template.DefaultName, validators);
            OnPropertyChanged("ItemName");
            MainViewModel.Current.CleanStatus(true);
        }

        private void UpdateHeader(int templatesCount)
        {
            if (MainViewModel.Current.ConfigTemplateType == TemplateType.Page)
            {
                Header = String.Format(StringRes.GroupPageHeader_SF, templatesCount);
            }
            else if (MainViewModel.Current.ConfigTemplateType == TemplateType.Feature)
            {
                Header = String.Format(StringRes.GroupFeatureHeader_SF, templatesCount);
            }
        }

        private void UpdateItemName(string name)
        {
            var validators = new List<Validator>()
            {
                new DefaultNamesValidator(),
                new ReservedNamesValidator()
            };

            var validationResult = Naming.Validate(name, validators);
            if (!validationResult.IsValid)
            {
                var errorMessage = StringRes.ResourceManager.GetString($"ValidationError_{validationResult.ErrorType}");

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "UndefinedError";
                }
                MainViewModel.Current.SetValidationErrors(errorMessage);
                throw new Exception(errorMessage);
            }
            MainViewModel.Current.CleanStatus(true);
        }
    }
}
