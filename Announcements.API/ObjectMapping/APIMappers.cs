using Announcements.API.Entities.Announcements;
using Announcements.API.Services.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
namespace Announcements.API.ObjectMapping;
/*
 * You can add your own mappings here.
 * [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
 * public partial class APIMappers : MapperBase<BookDto, CreateUpdateBookDto>
 * {
 *    public override partial CreateUpdateBookDto Map(BookDto source);
 *
 *    public override partial void Map(BookDto source, CreateUpdateBookDto destination);
 * }
 */

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class APIAnnouncementToAnnouncementDtoMapper : MapperBase<Announcement, AnnouncementDto>
{
    public override partial AnnouncementDto Map(Announcement source);

    public override partial void Map(Announcement source, AnnouncementDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class APIAnnouncementDtoToAnnouncementMapper : MapperBase<AnnouncementDto, Announcement>
{
    public override partial Announcement Map(AnnouncementDto source);

    public override partial void Map(AnnouncementDto source, Announcement destination);
}
