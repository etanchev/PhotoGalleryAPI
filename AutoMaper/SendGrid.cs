using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhotoGalleryAPI.Models;

namespace PhotoGalleryAPI.AutoMaper
{
    public class SendGrid : Profile
    {
        public SendGrid()
        {
            CreateMap<SendGridDbModel, Dto.SendGridParserDto>();
            CreateMap<SendGridPostModel, SendGridDbModel>();
            CreateMap<EmailModel, EmailDbModel>();
            CreateMap<PushSubscribe, PushSubscribeDbModel>().IncludeMembers(s => s.Keys);
            CreateMap<Keys, PushSubscribeDbModel>();

        }
    }
}
