﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Mvvm;
using Microsoft.Templates.Core.PostActions.Catalog.Merge;
using Microsoft.Templates.UI.V2Resources;
using Microsoft.Templates.UI.V2ViewModels.Common;

namespace Microsoft.Templates.UI.V2ViewModels.NewItem
{
    public class ChangesSummaryViewModel : Observable
    {
        private NewItemFileViewModel _selected;

        public NewItemFileViewModel Selected
        {
            get => _selected;
            private set => SetProperty(ref _selected, value);
        }

        public ObservableCollection<ItemsGroupViewModel<NewItemFileViewModel>> FileGroups { get; } = new ObservableCollection<ItemsGroupViewModel<NewItemFileViewModel>>();

        public ChangesSummaryViewModel()
        {
        }

        public void Initialize(NewItemGenerationResult output)
        {
            var warnings = GenContext.Current.FailedMergePostActions.Where(w => w.MergeFailureType == MergeFailureType.FileNotFound || w.MergeFailureType == MergeFailureType.LineNotFound);
            var failedStyleMerges = GenContext.Current.FailedMergePostActions.Where(w => w.MergeFailureType == MergeFailureType.KeyAlreadyDefined);

            FileGroups.Clear();
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupWarningFiles, warnings.Select(f => NewItemFileViewModel.WarningFile(f))));
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupConflictingStylesFiles, failedStyleMerges.Select(f => NewItemFileViewModel.ConflictingStylesFile(f))));
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupNewFiles, output.NewFiles.Select(f => NewItemFileViewModel.NewFile(f))));
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupModifiedFiles, output.ModifiedFiles.Select(f => NewItemFileViewModel.ModifiedFile(f))));
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupUnchangedFiles, output.UnchangedFiles.Select(f => NewItemFileViewModel.UnchangedFile(f))));
            AddGroup(new ItemsGroupViewModel<NewItemFileViewModel>(StringRes.ChangesSummaryGroupConflictingFiles, output.ConflictingFiles.Select(f => NewItemFileViewModel.ConflictingFile(f))));

            SelectFile(FileGroups.First().Items.First());
        }

        private void AddGroup(ItemsGroupViewModel<NewItemFileViewModel> group)
        {
            if (group.Items.Any())
            {
                FileGroups.Add(group);
            }
        }

        public void SelectFile(NewItemFileViewModel file)
        {
            foreach (var group in FileGroups)
            {
                group.CleanSelected();
            }

            Selected = file;
            Selected.IsSelected = true;
        }
    }
}