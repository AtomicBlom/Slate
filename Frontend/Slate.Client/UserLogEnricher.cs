using Serilog.Core;
using Serilog.Events;

namespace Slate.Client
{
    public class UserLogEnricher : ILogEventEnricher, IUserLogEnricher
    {
        private string? _userId;
        private string? _characterId;
        private string? _previousUserId;
        private string? _previousCharacterId;
        private LogEventProperty? _userIdProperty;
        private LogEventProperty? _characterIdProperty;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!Equals(_userId, _previousUserId))
            {
                _userIdProperty = new LogEventProperty("UserId", new ScalarValue(_userId));
                _previousUserId = _userId;
            }

            if (!Equals(_characterId, _previousCharacterId))
            {
                _characterIdProperty = new LogEventProperty("CharacterId", new ScalarValue(_characterId));
                _previousCharacterId = _characterId;
            }

            if (_userIdProperty is not null) logEvent.AddOrUpdateProperty(_userIdProperty);
            if (_characterIdProperty is not null) logEvent.AddOrUpdateProperty(_characterIdProperty);
        }

        public string UserId
        {
            set { _userId = value; }
        }
        public string CharacterId
        {
            set { _characterId = value; }
        }

        public void Reset()
        {
            _userId = _previousUserId = _characterId = _previousCharacterId = null;
            _userIdProperty = null;
            _characterIdProperty = null;
        }
    }
}