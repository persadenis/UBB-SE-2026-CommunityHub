using System;
using ChatModule.ViewModels;
using Xunit;

namespace ChatModule.Tests
{
    public class BaseViewModelTests
    {
        [Fact]
        public void Set_SameValue_ReturnsFalse()
        {
            var vm = new TestableBaseViewModel();
            var field = "same";

            var changed = vm.SetValue(ref field, "same", "Field");

            Assert.False(changed);
        }

        [Fact]
        public void Set_DifferentValue_UpdatesField()
        {
            var vm = new TestableBaseViewModel();
            var field = "old";

            vm.SetValue(ref field, "new", "Field");

            Assert.Equal("new", field);
        }

        [Fact]
        public void Set_DifferentValue_RaisesPropertyChanged()
        {
            var vm = new TestableBaseViewModel();
            string? raisedPropertyName = null;

            vm.PropertyChanged += (_, e) => raisedPropertyName = e.PropertyName;
            vm.Name = "updated";

            Assert.Equal("Name", raisedPropertyName);
        }

        [Fact]
        public void OnPropertyChanged_RaisesEventWithProvidedName()
        {
            var vm = new TestableBaseViewModel();
            string? raisedPropertyName = null;

            vm.PropertyChanged += (_, e) => raisedPropertyName = e.PropertyName;
            vm.RaiseChanged("ManualProperty");

            Assert.Equal("ManualProperty", raisedPropertyName);
        }

        private sealed class TestableBaseViewModel : BaseViewModel
        {
            private string _name = string.Empty;

            public string Name
            {
                get => this._name;
                set => this.Set(ref this._name, value);
            }

            public bool SetValue(ref string field, string value, string? name)
            {
                return this.Set(ref field, value, name);
            }

            public void RaiseChanged(string? name)
            {
                this.OnPropertyChanged(name);
            }
        }
    }
}