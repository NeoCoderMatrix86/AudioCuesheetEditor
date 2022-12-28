//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using AudioCuesheetEditor.Model.Entity;
using Blazorise;
using Blazorise.Localization;
using System.Linq.Expressions;

namespace AudioCuesheetEditor.Model.UI
{
    public class ValidatorUtility<T> where T : IValidateable<T>
    {
        public static Task Validate<TProperty>(ValidatorEventArgs args, T? entity, Expression<Func<T, TProperty>> expression, ITextLocalizer<IValidateable> textLocalizer, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (entity != null)
                {
                    var validationResult = entity.Validate(expression);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        switch (validationResult.Status)
                        {
                            case Entity.ValidationStatus.NoValidation:
                                args.Status = Blazorise.ValidationStatus.None;
                                break;
                            case Entity.ValidationStatus.Success:
                                args.Status = Blazorise.ValidationStatus.Success;
                                break;
                            case Entity.ValidationStatus.Error:
                                args.Status = Blazorise.ValidationStatus.Error;
                                if (validationResult.ErrorMessages != null)
                                {
                                    foreach (var error in validationResult.ErrorMessages)
                                    {
                                        args.ErrorText += String.Format("{0}{1}", textLocalizer[error], Environment.NewLine);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
