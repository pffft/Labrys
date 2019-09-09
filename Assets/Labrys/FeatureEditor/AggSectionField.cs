using System.Collections.Generic;

namespace Labrys.FeatureEditor
{
    public class AggSectionField : IField
    {
        public List<IField> Fields { get; set; } = new List<IField>();

        public string Name
        {
            get
            {
                string aggregate = "";
                foreach (IField f in Fields)
                {
                    if (aggregate == "")
                        aggregate = f.Name;
                    else if (aggregate != f.Name)
                    {
                        aggregate = "-";
                        return aggregate;
                    }
                }
                return aggregate;
            }

            set
            {
                foreach (IField f in Fields)
                {
                    f.Name = value;
                }
            }
        }

        public string Value
        {
            get
            {
                string aggregate = "";
                foreach (IField f in Fields)
                {
                    if (aggregate == "")
                        aggregate = f.Value;
                    else if (aggregate != f.Value)
                    {
                        aggregate = "-";
                        return aggregate;
                    }
                }
                return aggregate;
            }

            set
            {
                foreach (IField f in Fields)
                {
                    f.Value = value;
                }
            }
        }
    }
}
