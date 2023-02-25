using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoopBackExample
{
    public class Lbql
    {
        public Dictionary<string, object> Where { get; set; }
        public List<KeyValuePair<string, string>> Order { get; set; }
        public int Limit { get; set; }
        public List<string> Fields { get; set; }
        public int Skip { get; set; }

        public void Parse(IQueryCollection query)
        {
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            foreach(var q in query)
            {
                queryParameters.Add(q.Key, q.Value);
            }
            Where = new Dictionary<string, object>();
            Order = new List<KeyValuePair<string, string>>();
            Fields = new List<string>();

            foreach (var queryParameter in queryParameters)
            {
                var parameterNameParts = queryParameter.Key.Split(new char[] { '[', ']' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parameterNameParts.Length > 0)
                {
                    if (!parameterNameParts[0].Equals("filter", StringComparison.InvariantCultureIgnoreCase)) continue;
                    var parameterName = parameterNameParts[1];
                    var parameterValue = queryParameter.Value;

                    switch (parameterName)
                    {
                        case "where":
                            ParseWhereParameter(parameterNameParts[2], parameterNameParts[3], parameterValue);
                            break;

                        case "order":
                            ParseOrderParameter(parameterValue);
                            break;

                        case "limit":
                            ParseLimitParameter(parameterValue);
                            break;

                        case "fields":
                            ParseFieldsParameter(parameterValue);
                            break;

                        case "skip":
                            ParseSkipParameter(parameterValue);
                            break;
                    }
                }
            }
        }

        private void ParseWhereParameter(string fieldName, string lbqlOperator, string fieldValue)
        {
            switch(lbqlOperator)
            {
                case "eq":
                    Where[fieldName] = fieldValue;
                    break;
            }
        }

        private void ParseJsonWhereParameter(string parameterValue)
        {
            var whereObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(parameterValue);
            if (whereObject != null)
            {
                foreach (var whereClause in whereObject)
                {
                    var fieldName = whereClause.Key;
                    var fieldValue = whereClause.Value;

                    if (fieldValue is Dictionary<string, object>)
                    {
                        var comparisonOperator = ((Dictionary<string, object>)fieldValue).First().Key;
                        var comparisonValue = ((Dictionary<string, object>)fieldValue).First().Value;

                        switch (comparisonOperator)
                        {
                            case "eq":
                                Where[fieldName] = comparisonValue;
                                break;

                            case "neq":
                                Where[$"{fieldName}!"] = comparisonValue;
                                break;

                            case "lt":
                                Where[$"{fieldName}<"] = comparisonValue;
                                break;

                            case "lte":
                                Where[$"{fieldName}<="] = comparisonValue;
                                break;

                            case "gt":
                                Where[$"{fieldName}>"] = comparisonValue;
                                break;

                            case "gte":
                                Where[$"{fieldName}>="] = comparisonValue;
                                break;

                            case "like":
                                Where[$"{fieldName}~"] = comparisonValue;
                                break;
                        }
                    }
                    else
                    {
                        Where[fieldName] = fieldValue;
                    }
                }
            }
        }

        private void ParseOrderParameter(string parameterValue)
        {
            var orderArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(parameterValue);
            if (orderArray != null)
            {
                foreach (var order in orderArray)
                {
                    if (order.StartsWith("-"))
                    {
                        Order.Add(new KeyValuePair<string, string>(order.Substring(1), "desc"));
                    }
                    else
                    {
                        Order.Add(new KeyValuePair<string, string>(order, "asc"));
                    }
                }
            }
        }

        private void ParseLimitParameter(string parameterValue)
        {
            int.TryParse(parameterValue, out int limit);
            Limit = limit;
        }

        private void ParseFieldsParameter(string parameterValue)
        {
            var fieldsArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(parameterValue);
            if (fieldsArray != null)
            {
                Fields = fieldsArray;
            }
        }

        private void ParseSkipParameter(string parameterValue)
        {
            int.TryParse(parameterValue, out int skip);
            Skip = skip;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Parsed query parameters:\n");

            if (Where != null && Where.Count > 0)
            {
                sb.Append($"- Where: {Newtonsoft.Json.JsonConvert.SerializeObject(Where)}\n");
            }

            if (Order != null && Order.Count > 0)
            {
                var orderFields = Order.Select(o => o.Key);
                var orderDirection = Order.Select(o => o.Value);
                sb.Append($"- Order by: {string.Join(",", orderFields)} {string.Join(",", orderDirection)}\n");
            }

            if (Limit > 0)
            {
                sb.Append($"- Limit: {Limit}\n");
            }

            if (Fields != null && Fields.Count > 0)
            {
                sb.Append($"- Fields: {string.Join(",", Fields)}\n");
            }

            if (Skip > 0)
            {
                sb.Append($"- Skip: {Skip}\n");
            }

            return sb.ToString();
        }

    }
}
