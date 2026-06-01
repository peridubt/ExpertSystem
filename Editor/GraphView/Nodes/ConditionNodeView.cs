// Узел условия в графе. Даёт выбрать тип факта, его поле, оператор сравнения и
// значение. Имеет выходной порт для подключения к логическому узлу или корню.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpertSystem.Rules.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExpertSystem.Editor.GraphView.Nodes
{
    /// <summary>Визуальный узел одного условия (паттерна над фактом).</summary>
    public class ConditionNodeView : Node
    {
        public ConditionData Data { get; }
        public Port OutputPort { get; }

        private PopupField<Type> _factDropdown;
        private PopupField<MemberInfo> _memberDropdown;
        private EnumField _operatorField;
        private TextField _valueField;

        /// <summary>Строит узел из данных условия. Принимает модель ConditionData.</summary>
        public ConditionNodeView(ConditionData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            title = "Condition";

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            OutputPort.portName = "When";
            outputContainer.Add(OutputPort);

            // Выпадающий список типов фактов.
            var factTypes = FactTypeRegistry.FactTypes.ToList();
            var initialFact = factTypes.FirstOrDefault(t => t.Name == data.factTypeName) ?? factTypes.FirstOrDefault();
            _factDropdown = new PopupField<Type>("Fact Type", factTypes, initialFact, FormatType, FormatType);
            _factDropdown.RegisterValueChangedCallback(evt => OnFactChanged(evt.newValue));
            extensionContainer.Add(_factDropdown);

            RebuildMemberDropdown(initialFact, data.fieldName);

            _operatorField = new EnumField("Operator", data.op);
            _operatorField.RegisterValueChangedCallback(evt => { Data.op = (ComparisonOperator)evt.newValue; });
            extensionContainer.Add(_operatorField);

            _valueField = new TextField("Value") { value = data.constantValue ?? string.Empty };
            _valueField.RegisterValueChangedCallback(evt => Data.constantValue = evt.newValue);
            extensionContainer.Add(_valueField);

            if (initialFact != null) Data.factTypeName = initialFact.Name;

            SetPosition(new Rect(data.position, new Vector2(220, 0)));
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>Реакция на смену типа факта: сбрасывает поле и пересобирает список полей.</summary>
        private void OnFactChanged(Type newType)
        {
            Data.factTypeName = newType?.Name ?? string.Empty;
            Data.fieldName = string.Empty;
            RebuildMemberDropdown(newType, null);
        }

        /// <summary>
        /// Пересобирает выпадающий список полей выбранного факта. Принимает тип факта и
        /// предпочтительное имя поля (для восстановления выбора).
        /// </summary>
        private void RebuildMemberDropdown(Type factType, string preferredField)
        {
            if (_memberDropdown != null && _memberDropdown.parent != null)
            {
                _memberDropdown.parent.Remove(_memberDropdown);
            }

            var members = factType == null
                ? new List<MemberInfo>()
                : FactTypeRegistry.GetMembers(factType).ToList();

            var initialMember = members.FirstOrDefault(m => m.Name == preferredField) ?? members.FirstOrDefault();
            _memberDropdown = new PopupField<MemberInfo>("Property", members, initialMember, FormatMember, FormatMember);
            _memberDropdown.RegisterValueChangedCallback(evt =>
            {
                Data.fieldName = evt.newValue?.Name ?? string.Empty;
            });

            // Вставляем список полей сразу под списком типов.
            int factIndex = extensionContainer.IndexOf(_factDropdown);
            extensionContainer.Insert(factIndex + 1, _memberDropdown);

            if (initialMember != null) Data.fieldName = initialMember.Name;
        }

        /// <summary>Отображение типа в списке.</summary>
        private static string FormatType(Type t) => t == null ? "<none>" : t.Name;

        /// <summary>Отображение члена в списке.</summary>
        private static string FormatMember(MemberInfo m) => m == null ? "<none>" : m.Name;

        /// <summary>Запоминает позицию узла в данных при перемещении.</summary>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Data.position = newPos.position;
        }
    }
}
