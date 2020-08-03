using System;
using System.Collections.Generic;
using System.Text;

namespace Qwasi.GraphUI
{
    public interface IQGUserDeletable : IInterfaceStateAgent<IQGUserDeletable>
    {
        public static readonly VariableIdentifier<IQGUserDeletable> UserDeletableProperty =
            VariableIdentifier<IQGUserDeletable>.Create("UserDeletable");

        public bool UserDeletable
        {
            get => (bool)GetInstance(UserDeletableProperty, true);
            set => SetInstance(UserDeletableProperty, value);
        }

        public void UserIssuedDelete()
        {
            if (this.UserDeletable && this is IQGControl asControl)
                asControl.DeleteControl();
        }
    }
}
