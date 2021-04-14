using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Model
{
    public enum AGECATEGORY
    {
        YOUNG = 0,
        SEMI = 1,
        ADULT = 2
    }

    public enum GENDER
    {
        FEMALE = 0,
        MALE = 1,
        OTHER = 2,
        ANY = 3
    }

    public enum CHATTPYE
    {
        GROUP,
        PRIVATE
    }
}
