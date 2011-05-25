using System;

namespace Scratch
{
    public class Events
    {
        public class RegisteredEvent : CVEvent
        {
            public RegisteredEvent():base()
            {
                UserGroupId = Guid.NewGuid();
                UserId = Guid.NewGuid();
                Email = "satnoehusatnoeh@aoeutnh.ent";
                Ssn = "761206-0272";
                UserAgreementReadOn = DateTime.UtcNow;
            }

            public Guid UserGroupId { get; set; }
            public Guid UserId { get; set; }
            public string Email { get; set; }
            public string Ssn { get; set; }
            public DateTime UserAgreementReadOn { get; set; }
        }

        public class AddSkillEvent : CVEvent
        {
            public AddSkillEvent():base()
            {
                SkillId = Guid.NewGuid();
            }

            public Guid SkillId { get; set; }
        }

        public class CVEvent
        {
            public Guid CandidateId { get; set; }

            protected CVEvent()
            {
                CandidateId = Guid.Parse("3A42AD57-B48A-489D-8628-7C25F0AC6D58");
            }
        }
    }
}